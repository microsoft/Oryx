# Build PHP 8.1
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php81-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP81_VERSION GPG_KEYS=$PHP81_KEYS PHP_SHA256=$PHP81_TAR_SHA256 /php/build.sh