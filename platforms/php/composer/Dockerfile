ARG OS_FLAVOR
ARG BUILDPACK_DEPS_REGISTRY=mcr.microsoft.com/mirror/docker/library
FROM ${BUILDPACK_DEPS_REGISTRY}/buildpack-deps:${OS_FLAVOR} AS php-buildpack-prereqs
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=$VERSIONS_TO_BUILD_OVERRIDE
COPY platforms/php/prereqs /php
# COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

ARG PHP81_VERSION='8.1.29'
ARG PHP81_KEYS='528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544'
ARG PHP81_TAR_SHA256='288884af60581d4284baba2ace9ca6d646f72facbd3e3c2dd2acc7fe6f903536'
ARG PHP86_VERSION='8.6.0beta1'
ARG PHP86_KEYS='D95C03BC702BE9515344AE3374E44BC9067701A5 016895DE9A475111D537A6E69134FF30BC5A99B5 5CFF17B64DC1C244F5D0EAC3E43535E2EB19010E'
ARG PHP86_TAR_SHA256='e44571d75b368b36a2d6db6878705e5512af5d2a78e31463c49796a94018d282'

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN export OS_FLAVOR=${OS_FLAVOR} ; \
        . /php/installPrereqs.sh ;

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN if [ "${OS_FLAVOR}" = "resolute" ]; then \
        PHP_VERSION=${PHP86_VERSION} GPG_KEYS="${PHP86_KEYS}" PHP_SHA256=${PHP86_TAR_SHA256} \
            /tmp/platforms/php/buildPhp.sh ${OS_FLAVOR} "" "php-composer"; \
    else \
        PHP_VERSION=${PHP81_VERSION} GPG_KEYS="${PHP81_KEYS}" PHP_SHA256=${PHP81_TAR_SHA256} \
            /tmp/platforms/php/buildPhp.sh ${OS_FLAVOR} "" "php-composer"; \
    fi