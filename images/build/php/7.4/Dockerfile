# Build PHP 7.4
FROM php-build-prereqs AS php74-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP74_VERSION GPG_KEYS=$PHP74_KEYS PHP_SHA256=$PHP74_TAR_SHA256 /php/build.sh

# Install Composer so that it can be copied to the final build image later
RUN set -eux && . /php/__phpVersions.sh && \
    mkdir /opt/php-composer && \
    export phpbin="/opt/php/$PHP74_VERSION/bin/php" && \
    $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" && \
    $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir=/opt/php-composer && \
    $phpbin -r "unlink('composer-setup.php');"
