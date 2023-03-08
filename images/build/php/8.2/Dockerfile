# Build PHP 8.2
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php82-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP82_VERSION GPG_KEYS=$PHP82_KEYS PHP_SHA256=$PHP82_TAR_SHA256 /php/build.sh