FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs
ARG PHP_VERSION
ARG GPG_KEYS
ARG PHP_SHA256
ARG COMPOSER_VERSION
ARG COMPOSER_SETUP_SHA384
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
RUN PHP_VERSION=$PHP_VERSION \
    GPG_KEYS=$GPG_KEYS \
    PHP_SHA256=$PHP_SHA256 \
    /php/build.sh

RUN set -eux; \
    composerDir="/opt/php-composer"; \
    mkdir -p "$composerDir"; \
    export phpbin="/opt/php/$PHP_VERSION/bin/php" ; \
    $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" ; \
    $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SETUP_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" ; \
    $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir="$composerDir" ; \
    $phpbin -r "unlink('composer-setup.php');" ; \
    compressedSdkDir="/tmp/compressedSdk"; \
    mkdir -p "$compressedSdkDir"; \
    cd "$composerDir"; \
    echo 'debian flavor is: $DEBIAN_FLAVOR' ; \
    composerSdkFile="php-composer-$DEBIAN_FLAVOR-$COMPOSER_VERSION.tar.gz"; \
    if [ "$DEBIAN_FLAVOR" = "stretch" ]; then \
       echo 'somehow debian flavor is: $DEBIAN_FLAVOR'; \
       composerSdkFile="php-composer-$COMPOSER_VERSION.tar.gz"; \ 
    fi;  \
    tar -zcf "$compressedSdkDir/$composerSdkFile" .
 
