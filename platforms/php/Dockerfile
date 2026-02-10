ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR} AS php-buildpack-prereqs
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
COPY platforms/php/prereqs /php
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/php/buildPhp.sh ${OS_FLAVOR} "" "php"