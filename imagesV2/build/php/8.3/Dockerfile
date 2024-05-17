# Build PHP 8.3
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php83-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP83_VERSION GPG_KEYS=$PHP83_KEYS PHP_SHA256=$PHP83_TAR_SHA256 /php/build.sh