ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR}
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=$VERSIONS_TO_BUILD_OVERRIDE

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
ENV NVM_DIR /usr/local/nvm
RUN mkdir -p $NVM_DIR \
    && curl -sL https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.4/install.sh | bash

RUN mkdir -p /tmp/scripts
COPY platforms/nodejs/build.sh /tmp/scripts
RUN chmod +x /tmp/scripts/build.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/nodejs/getNode.sh ${OS_FLAVOR} ""