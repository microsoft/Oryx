FROM php-build-prereqs
ARG PHP_VERSION
ARG GPG_KEYS
ARG PHP_SHA256
RUN PHP_VERSION=$PHP_VERSION \
    GPG_KEYS=$GPG_KEYS \
    PHP_SHA256=$PHP_SHA256 \
    /php/build.sh

RUN set -eux \
 && . /php/__phpVersions.sh \
 && composerDir="/opt/php-composer" \
 && mkdir -p "$composerDir" \
 && export phpbin="/opt/php/$PHP_VERSION/bin/php" \
 && $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" \
 && $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" \
 && $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir="$composerDir" \
 && $phpbin -r "unlink('composer-setup.php');" \
 && compressedSdkDir="/tmp/compressedSdk" \
 && cd "$composerDir" \
 && tar -zcf "$compressedSdkDir/php-composer-$COMPOSER_VERSION.tar.gz" .
