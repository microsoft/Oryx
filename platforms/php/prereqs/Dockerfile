ARG PHP_FLAVOR
ARG DEBIAN_FLAVOR
FROM php:${PHP_FLAVOR} as busterLibs
ARG PHP_FLAVOR
ARG DEBIAN_FLAVOR
ENV PHP_FLAVOR=$PHP_FLAVOR

RUN set -ex \
 && echo "value of PHP_FLAVOR is ${PHP_FLAVOR}" \
 && echo "busterlibs"

ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS php-build-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
    apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        #curl \
        xz-utils \
        libsodium-dev \
        libncurses5 \
    --no-install-recommends && rm -r /var/lib/apt/lists/* ; \
    else \
        . /php/installPrereqs.sh ; \
    fi

COPY --from=busterLibs /usr/lib/x86_64-linux-gnu /usr/lib/x86_64-linux-gnu