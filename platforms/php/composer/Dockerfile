ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR} AS php-buildpack-prereqs
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
COPY platforms/php/prereqs /php
# COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

ARG PHP81_VERSION='8.1.29'
ARG PHP81_KEYS='528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544'
ARG PHP81_TAR_SHA256='288884af60581d4284baba2ace9ca6d646f72facbd3e3c2dd2acc7fe6f903536'

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN export OS_FLAVOR=${OS_FLAVOR} ; \
        . /php/installPrereqs.sh ;

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN PHP_VERSION=${PHP81_VERSION} GPG_KEYS=${PHP81_KEYS} PHP_SHA256=${PHP81_TAR_SHA256} /tmp/platforms/php/buildPhp.sh ${OS_FLAVOR} "" "php-composer"