ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_FLAVOR} AS php-buildpack-prereqs
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/php/buildPhp.sh debianFlavor=${DEBIAN_FLAVOR} sdkStorageAccountUrl="" phpType="php"

# ARG PHP_VERSION
# ARG GPG_KEYS
# ARG PHP_SHA256
# ARG DEBIAN_FLAVOR
# ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

# RUN PHP_VERSION=$PHP_VERSION \
#     GPG_KEYS=$GPG_KEYS \
#     PHP_SHA256=$PHP_SHA256 \
#     /php/build.sh