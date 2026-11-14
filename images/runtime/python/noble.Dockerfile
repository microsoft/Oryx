ARG OS_TYPE=ubuntu
ARG OS_FLAVOR=noble
ARG INCLUDE_AZURELINUX_CERTS=true

# Stage: (optional) pull Azure Linux CA certificates so we can merge them into BaseOS trust store
# Debian images (especially slim variants) can lag in ca-certificates updates; we optionally add Azure Linux certs.
FROM mcr.microsoft.com/azurelinux/base/core:3.0 AS azurelinux-core
ARG INCLUDE_AZURELINUX_CERTS
COPY images/runtime/certs/blocklist/ /etc/pki/ca-trust/source/blacklist/
COPY images/runtime/scripts/install-azurelinux-ca-packages.sh /usr/local/bin/
RUN if [ "$(printf '%s' "$INCLUDE_AZURELINUX_CERTS" | tr '[:upper:]' '[:lower:]')" = "true" ]; then \
		set -eux; \
		sh /usr/local/bin/install-azurelinux-ca-packages.sh; \
	fi

FROM mcr.microsoft.com/mirror/docker/library/${OS_TYPE}:${OS_FLAVOR} AS oryx_runtime_base
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
ARG INCLUDE_AZURELINUX_CERTS
ARG OS_FLAVOR

COPY images/runtime/scripts/configure-debian-bullseye-snapshot.sh /usr/local/bin/
# TODO: Remove this once Debian Bullseye and Bookworm provide tzdata 2026e-1 or newer.
COPY images/runtime/scripts/install-debian-tzdata-2026e.sh /usr/local/bin/
RUN if [ "${OS_FLAVOR}" = "bullseye" ]; then \
		chmod +x /usr/local/bin/configure-debian-bullseye-snapshot.sh; \
		/usr/local/bin/configure-debian-bullseye-snapshot.sh; \
	fi

RUN apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y --no-install-recommends \
		xz-utils \
		ca-certificates \
		curl \
		gnupg \
		netbase \
		wget

RUN if [ "${OS_FLAVOR}" = "bullseye" ] || [ "${OS_FLAVOR}" = "bookworm" ]; then \
		chmod +x /usr/local/bin/install-debian-tzdata-2026e.sh; \
		/usr/local/bin/install-debian-tzdata-2026e.sh; \
	elif [ "${OS_FLAVOR}" = "noble" ] || [ "${OS_FLAVOR}" = "resolute" ]; then \
		apt-get update; \
		DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends tzdata; \
	fi

RUN if [ "${OS_FLAVOR}" = "bookworm" ] ; then \
    apt-get update && apt-get install -y --no-install-recommends sq ; \
    fi

RUN rm -f /usr/local/bin/configure-debian-bullseye-snapshot.sh /usr/local/bin/install-debian-tzdata-2026e.sh \
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

ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm as startupCmdGen

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
# Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh python /opt/startupcmdgen/startupcmdgen

# Build Python SDK from source in a disposable stage
FROM oryx_runtime_base AS pythonSdkBuilder
ARG OS_FLAVOR
ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ENV PYTHON_VERSION=${PYTHON_FULL_VERSION}
COPY platforms/python/prereqs/build.sh /tmp/build.sh
COPY platforms/python/versions/${OS_FLAVOR}/versionsToBuild.txt /tmp/versionsToBuild.txt
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh /tmp/receiveGpgKeys.sh
RUN set -e \
    && mkdir -p /usr/src/python && cd /usr/src/python \
    && VERSION_LINE=$(grep "^${PYTHON_VERSION}," /tmp/versionsToBuild.txt) \
    && export GPG_KEY=$(echo "$VERSION_LINE" | cut -d',' -f2 | tr -d ' ') \
    && export PYTHON_SHA256=$(echo "$VERSION_LINE" | cut -d',' -f3 | tr -d ' ') \
    && export OS_FLAVOR=${OS_FLAVOR} \
    && /tmp/build.sh

FROM oryx_runtime_base AS main

ARG IMAGES_DIR=/tmp/oryx/images

ARG OS_FLAVOR
ENV OS_FLAVOR=${OS_FLAVOR}

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        xz-utils \
        # Install gcc due to error installing viztracer returning gcc not found
        gcc \
    && rm -rf /var/lib/apt/lists/*

ADD images ${IMAGES_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

RUN ${IMAGES_DIR}/runtime/python/install-dependencies.sh

ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION

ENV PYTHON_VERSION ${PYTHON_FULL_VERSION}

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
    pip install --index-url $(cat /run/secrets/pip_index_url) gunicorn uvicorn==0.46.0 uvicorn-worker==0.4.0 debugpy viztracer==0.15.6 vizplugins==0.1.3 && \
    ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/oryx

ENV LANG="en_US.UTF-8" \
    LANGUAGE="en_US.UTF-8" \
    LC_ALL="en_US.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
