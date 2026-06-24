ARG DEBIAN_FLAVOR
ARG BASE_IMAGE

# Stage 1 — build the Oryx startup-script generator (the `oryx` CLI).
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


# Stage 2 — compile CPython from source (PGO+LTO via prereqs/build.sh).
FROM ${BASE_IMAGE} AS pythonSdkBuilder
ARG DEBIAN_FLAVOR
ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ENV PYTHON_VERSION=${PYTHON_FULL_VERSION}
COPY platforms/python/prereqs/build.sh /tmp/build.sh
COPY platforms/python/versions/${DEBIAN_FLAVOR}/versionsToBuild.txt /tmp/versionsToBuild.txt
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh /tmp/receiveGpgKeys.sh
RUN set -e \
    && mkdir -p /usr/src/python && cd /usr/src/python \
    && VERSION_LINE=$(grep "^${PYTHON_VERSION}," /tmp/versionsToBuild.txt) \
    && export GPG_KEY=$(echo "$VERSION_LINE" | cut -d',' -f2 | tr -d ' ') \
    && export PYTHON_SHA256=$(echo "$VERSION_LINE" | cut -d',' -f3 | tr -d ' ') \
    && export OS_FLAVOR=${DEBIAN_FLAVOR} \
    && /tmp/build.sh


# Stage 3 — final runtime image.
FROM ${BASE_IMAGE} as main

ARG SDK_STORAGE_BASE_URL_VALUE
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}
ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE}

# Layer 1 — runtime apt deps (least volatile; bumped only on driver/SSL CVEs).
# Targeted COPY of only the dep script — avoids invalidating this layer on
# every unrelated change under images/.
COPY images/runtime/python/install-dependencies-slim.sh /tmp/install-dependencies-slim.sh
RUN chmod +x /tmp/install-dependencies-slim.sh \
    && /tmp/install-dependencies-slim.sh \
    && rm -f /tmp/install-dependencies-slim.sh

# Layer 2 — extracted Python (changes every patch bump).
ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION
ENV PYTHON_VERSION=${PYTHON_FULL_VERSION}

COPY --from=pythonSdkBuilder /opt/python/${PYTHON_FULL_VERSION} /opt/python/${PYTHON_FULL_VERSION}

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

# AI + cert envs.
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

# Buildpacks contract.
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

# Layer 3 — pip toolchain (gunicorn only; rebuilt on pip bumps).
RUN --mount=type=secret,id=pip_index_url,target=/run/secrets/pip_index_url \
    pip install --index-url $(cat /run/secrets/pip_index_url) --upgrade pip && \
    pip install --index-url $(cat /run/secrets/pip_index_url) gunicorn

# C.UTF-8 is provided by libc — no `locales` package install required.
ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

# Layer 4 — oryx CLI (per-build, tiny — last for max cache hit on rebuilds).
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx
