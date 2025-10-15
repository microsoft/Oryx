ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR}
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        make \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN curl -sL https://git.io/n-install | bash -s -- -ny -

RUN mkdir -p /tmp/scripts
COPY platforms/nodejs/build.sh /tmp/scripts
RUN chmod +x /tmp/scripts/build.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/nodejs/getNode.sh ${OS_FLAVOR} ""