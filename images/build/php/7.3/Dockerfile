# Build PHP 7.3
FROM php-build-prereqs AS php73-build
ARG PHP_VERSION
ARG GPG_KEYS
ARG PHP_SHA256
RUN PHP_VERSION=$PHP_VERSION GPG_KEYS=$GPG_KEYS PHP_SHA256=$PHP_SHA256 /php/build.sh

# Install Composer so that it can be copied to the final build image later
RUN set -eux && . /php/__phpVersions.sh && \
    mkdir /opt/php-composer && \
    export phpbin="/opt/php/$PHP_VERSION/bin/php" && \
    $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" && \
    $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir=/opt/php-composer && \
    $phpbin -r "unlink('composer-setup.php');"
