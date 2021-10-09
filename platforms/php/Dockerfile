ARG PHP_FLAVOR
ARG DEBIAN_FLAVOR
FROM php:${PHP_FLAVOR} as busterLibs
ARG PHP_FLAVOR
ARG DEBIAN_FLAVOR
ENV PHP_FLAVOR=$PHP_FLAVOR

RUN set -ex \
 && echo "value of PHP_FLAVOR is ${PHP_FLAVOR}" \
 && echo "busterlibs" \
 && echo "busterlibs" > /usr/lib/x86_64-linux-gnu/.busterlibs

FROM php-build-prereqs AS php73-build
ARG PHP_VERSION
ARG GPG_KEYS
ARG PHP_SHA256
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

COPY --from=busterLibs /usr/lib/x86_64-linux-gnu /tmp/lib/x86_64-linux-gnu
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
      cp -a /tmp/lib/x86_64-linux-gnu/ /usr/lib/x86_64-linux-gnu/ ; \
      ls -la /usr/lib/x86_64-linux-gnu ; \
    fi

RUN PHP_VERSION=$PHP_VERSION \
    GPG_KEYS=$GPG_KEYS \
    PHP_SHA256=$PHP_SHA256 \
    /php/build.sh