ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_FLAVOR} AS php-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

ARG PHP81_VERSION='8.1.29'
ARG PHP81_KEYS='528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544'
ARG PHP81_TAR_SHA256='288884af60581d4284baba2ace9ca6d646f72facbd3e3c2dd2acc7fe6f903536'

RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
    fi

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
    apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses5 \
    --no-install-recommends && rm -r /var/lib/apt/lists/* ; \
    else \
        export DEBIAN_FLAVOR=${DEBIAN_FLAVOR} ; \
        . /php/installPrereqs.sh ; \
    fi

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN PHP81_VERSION=${PHP81_VERSION} PHP81_KEYS=${PHP81_KEYS} PHP81_TAR_SHA256=${PHP81_TAR_SHA256} /tmp/platforms/php/buildPhp.sh ${DEBIAN_FLAVOR} "" "php"

# ARG PHP_VERSION
# ARG GPG_KEYS
# ARG PHP_SHA256
# ARG DEBIAN_FLAVOR
# ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

# RUN PHP_VERSION=$PHP_VERSION \
#     GPG_KEYS=$GPG_KEYS \
#     PHP_SHA256=$PHP_SHA256 \
#     /php/build.sh