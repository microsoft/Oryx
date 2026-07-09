ARG BASE_IMAGE
ARG NODE_FULL_VERSION
ARG NODE_SHA256

# build the Oryx startup-script generator (the `oryx` CLI).
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26.4-bookworm as startupCmdGen

WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN chmod +x build.sh && ./build.sh node /opt/startupcmdgen/startupcmdgen


# Download Node.js directly from official source and verify SHA256
FROM ${BASE_IMAGE} AS nodeDownloader
ARG NODE24_VERSION
ARG NODE_FULL_VERSION
ARG NODE_SHA256
WORKDIR /tmp/node-download
RUN set -ex \
    && curl -fsSLO "https://nodejs.org/dist/v${NODE_FULL_VERSION}/node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" \
    && echo "${NODE_SHA256} node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" | sha256sum -c - \
    && mkdir -p /opt/nodejs \
    && tar -xJf "node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" -C /opt/nodejs --strip-components=1 \
    && rm "node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" \
    && rm -rf /tmp/node-download

#FROM oryxdevmcr.azurecr.io/private/oryx/oryx-node-run-base-bullseye:${BUILD_NUMBER}
FROM ${BASE_IMAGE}

RUN groupadd --gid 1001 node \
  && useradd --uid 1001 --gid node --shell /bin/bash --create-home node

ARG NODE_FULL_VERSION
ENV NODE_VERSION=${NODE_FULL_VERSION}
ENV NPM_CONFIG_LOGLEVEL=info

COPY --from=nodeDownloader /opt/nodejs/ /usr/local/
RUN ln -s /usr/local/bin/node /usr/local/bin/nodejs

COPY images/runtime/node/installDependencies.sh /tmp/installDependencies.sh

ARG NPM_VERSION
ARG PM2_VERSION
ARG NODE_APP_INSIGHTS_SDK_VERSION

RUN --mount=type=secret,id=npmrc,target=/run/secrets/npmrc \
    FEED_ACCESSTOKEN=$(cat /run/secrets/npmrc) && \
    echo "registry=https://pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/registry/" > /root/.npmrc && \
    echo "always-auth=true" >> /root/.npmrc && \
    echo "//pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/registry/:_authToken=${FEED_ACCESSTOKEN}" >> /root/.npmrc && \
    echo "//pkgs.dev.azure.com/msazure/one/_packaging/one_PublicPackages/npm/:_authToken=${FEED_ACCESSTOKEN}" >> /root/.npmrc && \
    chmod +x /tmp/installDependencies.sh && \
    PM2_VERSION=${PM2_VERSION} NODE_APP_INSIGHTS_SDK_VERSION=${NODE_APP_INSIGHTS_SDK_VERSION} /tmp/installDependencies.sh && \
    npm cache clean --force && \
    rm -rf /tmp/ && \
    rm -rf /root/.npmrc

# Install Yarn
ARG YARN_VERSION
ENV YARN_VERSION ${YARN_VERSION}
COPY images/yarn-v${YARN_VERSION}.tar.gz .
RUN mkdir -p /opt \
  && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/ \
  && ln -s /opt/yarn-v$YARN_VERSION/bin/yarn /usr/local/bin/yarn \
  && ln -s /opt/yarn-v$YARN_VERSION/bin/yarnpkg /usr/local/bin/yarnpkg \
  && rm yarn-v$YARN_VERSION.tar.gz


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
    && chmod a+x /opt/node-wrapper/node 

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

CMD [ "node" ]