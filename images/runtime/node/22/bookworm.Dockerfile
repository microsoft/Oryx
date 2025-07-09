ARG BASE_IMAGE

# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.23.8-bookworm as startupCmdGen

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

#FROM oryxdevmcr.azurecr.io/private/oryx/oryx-node-run-base-bookworm:${BUILD_NUMBER}
FROM ${BASE_IMAGE}

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

ARG NODE22_VERSION
ENV NODE_VERSION ${NODE22_VERSION}
ENV NPM_CONFIG_LOGLEVEL info
ARG BUILD_DIR=/tmp/oryx/build
ARG IMAGES_DIR=/tmp/oryx/images

COPY nodejs-bookworm-${NODE22_VERSION}.tar.gz .
RUN set -e \
    && mkdir -p /opt/nodejs/${NODE22_VERSION} \
    && tar -xzf nodejs-bookworm-${NODE22_VERSION}.tar.gz -C /usr/local \
    && rm nodejs-bookworm-${NODE22_VERSION}.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

ARG NPM_VERSION
ARG PM2_VERSION
ARG NODE_APP_INSIGHTS_SDK_VERSION

RUN npm install -g npm@${NPM_VERSION}

RUN PM2_VERSION=${PM2_VERSION} NODE_APP_INSIGHTS_SDK_VERSION=${NODE_APP_INSIGHTS_SDK_VERSION} ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

#stable is an alias for the latest stable release.
ARG YARN_VERSION="stable"
# Enable Corepack to manage Yarn and other package managers, Also Pre-cache and activate the specified Yarn version
RUN corepack enable && corepack prepare yarn@${YARN_VERSION} --activate

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


