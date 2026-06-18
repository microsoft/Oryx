ARG OS_FLAVOR
ARG BASE_IMAGE

# -----------------------------------------------------------------------------
# Stage 1: build the Oryx startup-script generator (the `oryx` CLI)
#
# Pin to a PATCHED Go toolchain. The compiled binary embeds its toolchain
# version (readable via `go version -m`), so image scanners flag Go stdlib CVEs
# even though Go is absent from the runtime. go1.26.2 was flagged for 4 reachable
# stdlib CVEs (GO-2026-5039/5037/4971/4918); all are fixed in go1.26.4.
# Bump this on Go security releases and rebuild the CLI (it is Python-agnostic).
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26.4-bookworm AS startupCmdGen

WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh python /opt/startupcmdgen/startupcmdgen



# -----------------------------------------------------------------------------
# Stage 2: compile CPython from source (disposable builder)
#   build.sh now parses versionsToBuild.txt itself (gpg key + sha256).
#   Inputs are passed as ENV (ARGs are not visible to scripts).
# -----------------------------------------------------------------------------
FROM ${BASE_IMAGE} AS pythonRuntimeBinariesBuilder
ARG OS_FLAVOR
ARG PYTHON_FULL_VERSION
ENV OS_FLAVOR=${OS_FLAVOR}
ENV PYTHON_VERSION=${PYTHON_FULL_VERSION}

COPY platforms/python/prereqs/build.sh /tmp/build.sh
COPY platforms/python/versions/${OS_FLAVOR}/versionsToBuild.txt /tmp/versionsToBuild.txt
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh /tmp/receiveGpgKeys.sh \
    && mkdir -p /usr/src/python && cd /usr/src/python \
    && VERSION_LINE=$(grep "^${PYTHON_VERSION}," /tmp/versionsToBuild.txt) \
    && export GPG_KEY=$(echo "$VERSION_LINE" | cut -d',' -f2 | tr -d ' ') \
    && export PYTHON_SHA256=$(echo "$VERSION_LINE" | cut -d',' -f3 | tr -d ' ') \
    && export OS_FLAVOR=${OS_FLAVOR} \
    && /tmp/build.sh



# -----------------------------------------------------------------------------
# Stage 3: the runtime image
#   Order is by layer volatility (least-volatile first):
#     1. apt runtime libs  (rarely change)
#     2. COPY python bin    (changes every version bump)
#     3. symlinks / ldconfig / PATH
#     4. pip toolchain
#     5. COPY oryx CLI      (per-build, tiny -> last)
# -----------------------------------------------------------------------------

FROM ${BASE_IMAGE} as main
ARG OS_FLAVOR
ENV OS_FLAVOR=${OS_FLAVOR}

COPY images/runtime/python/revamped-install-dependencies.sh /tmp/revamped-install-dependencies.sh
RUN chmod +x /tmp/revamped-install-dependencies.sh \
    && bash /tmp/revamped-install-dependencies.sh \
    && rm -f /tmp/revamped-install-dependencies.sh

ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION

ENV PYTHON_VERSION ${PYTHON_FULL_VERSION}
COPY --from=pythonRuntimeBinariesBuilder /opt/python/${PYTHON_FULL_VERSION} /opt/python/${PYTHON_FULL_VERSION}
RUN set -ex \
 && cd /opt/python/ \
 && ln -s ${PYTHON_FULL_VERSION} ${PYTHON_VERSION} \
 && ln -s ${PYTHON_VERSION} ${PYTHON_MAJOR_VERSION} \
 && echo /opt/python/${PYTHON_MAJOR_VERSION}/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "${PYTHON_MAJOR_VERSION}" = "3" ]; then cd /opt/python/${PYTHON_MAJOR_VERSION}/bin \
 && ln -nsf idle3 idle \
 && ln -nsf pydoc3 pydoc \
 && ln -nsf python3-config python-config; fi \
 && rm -rf /var/lib/apt/lists/*


ENV PATH="/opt/python/${PYTHON_MAJOR_VERSION}/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

RUN --mount=type=secret,id=pip_index_url,target=/run/secrets/pip_index_url \
    pip install --index-url $(cat /run/secrets/pip_index_url) --upgrade pip && \
    pip install --index-url $(cat /run/secrets/pip_index_url) gunicorn uvicorn==0.46.0 uvicorn-worker==0.4.0 && \
    ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/oryx

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen