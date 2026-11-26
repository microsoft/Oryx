ARG OS_TYPE=debian
ARG OS_FLAVOR=bullseye
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

FROM mcr.microsoft.com/mirror/docker/library/${OS_TYPE}:${OS_FLAVOR} AS oryx_run_base
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

FROM oryx_run_base AS oryx_runtime_base
ARG IMAGES_DIR=/tmp/oryx/images

RUN apt-get update \
  && apt-get install -y \
    unzip \
  && rm -rf /var/lib/apt/lists/*

# NOTE: This is a list of keys for ALL Node versions and also Yarn package installs.
# Receiving them once speeds up building of individual node versions as they all derive
# from this image.

# Gpg keys listed at https://github.com/nodejs/node

RUN ${IMAGES_DIR}/receiveGpgKeys.sh \
    6A010C5166006599AA17F08146C2130DFD2497F5

ARG YARN_VERSION
ENV YARN_VERSION ${YARN_VERSION}

COPY images/yarn-v${YARN_VERSION}.tar.gz .

RUN mkdir -p /opt \
  && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/ \
  && ln -s /opt/yarn-v$YARN_VERSION/bin/yarn /usr/local/bin/yarn \
  && ln -s /opt/yarn-v$YARN_VERSION/bin/yarnpkg /usr/local/bin/yarnpkg \
  && rm yarn-v$YARN_VERSION.tar.gz

# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bullseye as startupCmdGen

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN chmod +x build.sh && ./build.sh node /opt/startupcmdgen/startupcmdgen

# Download Node.js directly from official source
FROM oryx_runtime_base AS nodeDownloader
ARG NODE20_VERSION
WORKDIR /tmp/node-download
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates xz-utils gnupg \
    && curl -fsSLO "https://nodejs.org/dist/v${NODE20_VERSION}/node-v${NODE20_VERSION}-linux-x64.tar.xz" \
    && curl -fsSL "https://nodejs.org/dist/v${NODE20_VERSION}/SHASUMS256.txt.asc" -o SHASUMS256.txt.asc \
    && curl -fsSL "https://github.com/nodejs/release-keys/raw/HEAD/gpg/pubring.kbx" -o nodejs-keyring.kbx \
    && gpg --no-default-keyring --keyring="/tmp/node-download/nodejs-keyring.kbx" --decrypt SHASUMS256.txt.asc > SHASUMS256.txt \
    && grep "node-v${NODE20_VERSION}-linux-x64.tar.xz" SHASUMS256.txt > node.sha256 \
    && [ -s node.sha256 ] \
    && sha256sum -c node.sha256 \
    && mkdir -p /opt/nodejs \
    && tar -xJf "node-v${NODE20_VERSION}-linux-x64.tar.xz" -C /opt/nodejs --strip-components=1 \
    && rm -rf /tmp/node-download

FROM oryx_runtime_base

RUN groupadd --gid 1000 node \
  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

RUN ARCH= && dpkgArch="$(dpkg --print-architecture)" \
  && case "${dpkgArch##*-}" in \
    amd64) ARCH='x64';; \
    ppc64el) ARCH='ppc64le';; \
    s390x) ARCH='s390x';; \
    arm64) ARCH='arm64';; \
    armhf) ARCH='armv7l';; \
    i386) ARCH='x86';; \
    *) echo "unsupported architecture"; exit 1 ;; \
  esac

ARG NODE20_VERSION
ENV NODE_VERSION ${NODE20_VERSION}
ENV NPM_CONFIG_LOGLEVEL info
ARG BUILD_DIR=/tmp/oryx/build
ARG IMAGES_DIR=/tmp/oryx/images

COPY --from=nodeDownloader /opt/nodejs/ /usr/local/
RUN ln -s /usr/local/bin/node /usr/local/bin/nodejs

ARG NPM_VERSION
ARG PM2_VERSION
ARG NODE_APP_INSIGHTS_SDK_VERSION

RUN --mount=type=secret,id=npmrc,target=/run/secrets/npmrc \
    FEED_ACCESSTOKEN=$(cat /run/secrets/npmrc) && \
    echo "registry=https://pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/registry/" > /root/.npmrc && \
    echo "always-auth=true" >> /root/.npmrc && \
    echo "//pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/registry/:_authToken=${FEED_ACCESSTOKEN}" >> /root/.npmrc && \
    echo "//pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/:_authToken=${FEED_ACCESSTOKEN}" >> /root/.npmrc && \
    npm install -g npm@${NPM_VERSION} && \
    NPM_VERSION=${NPM_VERSION} PM2_VERSION=${PM2_VERSION} NODE_APP_INSIGHTS_SDK_VERSION=${NODE_APP_INSIGHTS_SDK_VERSION} ${IMAGES_DIR}/runtime/node/installDependencies.sh && \
    rm -rf /tmp/oryx && \
    rm -rf /root/.npmrc

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen

# Node wrapper is used to debug apps when node is executed indirectly, e.g. by npm.
COPY src/startupscriptgenerator/src/node/wrapper/node /opt/node-wrapper/
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && chmod a+x /opt/node-wrapper/node \
    && apt-get update \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/*

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

CMD [ "node" ]


