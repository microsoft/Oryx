ARG OS_TYPE=ubuntu
ARG OS_FLAVOR=resolute
ARG INCLUDE_AZURELINUX_CERTS=true

# Stage: (optional) pull Azure Linux CA certificates so we can merge them into BaseOS trust store
# Debian images (especially slim variants) can lag in ca-certificates updates; we optionally add Azure Linux certs.
FROM mcr.microsoft.com/azurelinux/base/core:3.0 AS azurelinux-core
ARG INCLUDE_AZURELINUX_CERTS
COPY images/runtime/certs/blocklist/ /etc/pki/ca-trust/source/blacklist/
RUN if [ "$(printf '%s' "$INCLUDE_AZURELINUX_CERTS" | tr '[:upper:]' '[:lower:]')" = "true" ]; then \
		set -eux; \
		tdnf makecache; \
		tdnf install -y ca-certificates; \
		update-ca-trust extract; \
		tdnf clean all; \
	fi

FROM mcr.microsoft.com/mirror/docker/library/${OS_TYPE}:${OS_FLAVOR} AS oryx_runtime_base
ARG INCLUDE_AZURELINUX_CERTS

RUN apt-get update \
	&& apt-get upgrade -y \
	&& DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
		xz-utils \
		ca-certificates \
		curl \
		gnupg \
		netbase \
		wget \
        unzip \
		zstd \
		# Not shipped in Resolute, but will be required by Node runtime. It is tiny, would save from unexpected runtime failures in other frameworks too
		libatomic1 \
		tzdata \
	&& rm -rf /var/lib/apt/lists/*

# Copy Azure linux certs to a temporary location
RUN mkdir -p /tmp/azurelinux-ca-certs && \
	chmod 755 /tmp/azurelinux-ca-certs;
COPY --from=azurelinux-core /etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem /tmp/azurelinux-ca-certs/tls-ca-bundle.pem

COPY images/runtime/scripts/install-azurelinux-certs.sh /usr/local/bin/install-azurelinux-certs.sh

# Add Azure Linux certs to Debian's CA store if the flag is set to true
RUN set -e; \
    if [ "$(printf '%s' "$INCLUDE_AZURELINUX_CERTS" | tr '[:upper:]' '[:lower:]')" = "true" ]; then \
        chmod +x /usr/local/bin/install-azurelinux-certs.sh; \
        /usr/local/bin/install-azurelinux-certs.sh /tmp/azurelinux-ca-certs /tmp/azurelinux-ca-certs/tls-ca-bundle.pem; \
    fi;

# Cleanup script and temporary files
RUN	rm -f /usr/local/bin/install-azurelinux-certs.sh; \
	rm -rf /tmp/azurelinux-ca-certs;

# -----------------------------------------------------------------------------
# Stage 1: build the Oryx startup-script generator (the `oryx` CLI)
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupCmdGen

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
FROM oryx_runtime_base AS pythonRuntimeBinariesBuilder
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

FROM oryx_runtime_base AS main
ARG OS_FLAVOR
ENV OS_FLAVOR=${OS_FLAVOR}

COPY images/runtime/python/resolute-based/install-dependencies.sh /tmp/install-dependencies.sh
RUN chmod +x /tmp/install-dependencies.sh \
    && bash /tmp/install-dependencies.sh \
    && rm -f /tmp/install-dependencies.sh

ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION

ENV PYTHON_VERSION ${PYTHON_FULL_VERSION}
COPY --from=pythonRuntimeBinariesBuilder /opt/python/${PYTHON_FULL_VERSION} /opt/python/${PYTHON_FULL_VERSION}
RUN set -ex \
 && cd /opt/python/ \
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
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/oryx

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx