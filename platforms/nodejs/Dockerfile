ARG OS_FLAVOR
# OS_FLAVOR of resolute is not currently there for buildpack-deps
ARG BASE_IMAGE_OS=${OS_FLAVOR}
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${BASE_IMAGE_OS}
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=$VERSIONS_TO_BUILD_OVERRIDE

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
        xz-utils \
        gnupg \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /tmp/scripts
COPY platforms/nodejs/build.sh /tmp/scripts
RUN chmod +x /tmp/scripts/build.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/nodejs/getNode.sh ${OS_FLAVOR} ""