# Build PHP 7.3
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php73-build
RUN set -eux \
    && . /php/__phpVersions.sh \
    && PHP_VERSION=$PHP73_VERSION GPG_KEYS=$PHP73_KEYS PHP_SHA256=$PHP73_TAR_SHA256 /php/build.sh

# Install Composer so that it can be copied to the final build image later
RUN set -eux && . /php/__phpVersions.sh && \
    mkdir /opt/php-composer && \
    export phpbin="/opt/php/$PHP73_VERSION/bin/php" && \
    $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SETUP_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" && \
    $phpbin composer-setup.php --version=$COMPOSER_DEFAULT_VERSION --install-dir=/opt/php-composer && \
    $phpbin -r "unlink('composer-setup.php');"
