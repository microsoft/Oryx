ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_FLAVOR} AS php-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
# COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

ARG PHP83_VERSION='8.3.15'
ARG PHP83_KEYS='528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544'
ARG PHP83_TAR_SHA256='3df5d45637283f759eef8fc3ce03de829ded3e200c3da278936a684955d2f94f'

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN export DEBIAN_FLAVOR=${DEBIAN_FLAVOR} ; \
        . /php/installPrereqs.sh ;

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN PHP_VERSION=${PHP83_VERSION} GPG_KEYS=${PHP83_KEYS} PHP_SHA256=${PHP83_TAR_SHA256} /tmp/platforms/php/buildPhp.sh ${DEBIAN_FLAVOR} "" "php-composer"

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
 
