# Build PHP 7.4
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php74-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP74_VERSION GPG_KEYS=$PHP74_KEYS PHP_SHA256=$PHP74_TAR_SHA256 /php/build.sh