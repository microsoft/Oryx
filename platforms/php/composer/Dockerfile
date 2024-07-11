ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_FLAVOR} AS php-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
# COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

ARG PHP81_VERSION
ENV PHP81_VERSION=$PHP81_VERSION
ARG PHP81_KEYS
ENV PHP81_KEYS=$PHP81_KEYS
ARG PHP81_TAR_SHA256
ENV PHP81_TAR_SHA256=$PHP81_TAR_SHA256

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
RUN /tmp/platforms/php/buildPhp.sh ${DEBIAN_FLAVOR} "" "php-composer" ${PHP81_VERSION} ${PHP81_KEYS} ${PHP81_TAR_SHA256}

# ARG PHP_VERSION
# ARG GPG_KEYS
# ARG PHP_SHA256
# ARG COMPOSER_VERSION
# ARG COMPOSER_SETUP_SHA384
# ARG DEBIAN_FLAVOR
# ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
# RUN PHP_VERSION=$PHP_VERSION \
#     GPG_KEYS=$GPG_KEYS \
#     PHP_SHA256=$PHP_SHA256 \
#     /php/build.sh

# RUN set -eux; \
#     composerDir="/opt/php-composer"; \
#     mkdir -p "$composerDir"; \
#     export phpbin="/opt/php/$PHP_VERSION/bin/php" ; \
#     $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" ; \
#     $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SETUP_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" ; \
#     $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir="$composerDir" ; \
#     $phpbin -r "unlink('composer-setup.php');" ; \
#     compressedSdkDir="/tmp/compressedSdk"; \
#     mkdir -p "$compressedSdkDir"; \
#     cd "$composerDir"; \
#     echo 'debian flavor is: $DEBIAN_FLAVOR' ; \
#     composerSdkFile="php-composer-$DEBIAN_FLAVOR-$COMPOSER_VERSION.tar.gz"; \
#     if [ "$DEBIAN_FLAVOR" = "stretch" ]; then \
#        echo 'somehow debian flavor is: $DEBIAN_FLAVOR'; \
#        composerSdkFile="php-composer-$COMPOSER_VERSION.tar.gz"; \ 
#     fi;  \
#     tar -zcf "$compressedSdkDir/$composerSdkFile" .
 
