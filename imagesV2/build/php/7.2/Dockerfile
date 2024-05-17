# Build PHP 7.2
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php72-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP72_VERSION GPG_KEYS=$PHP72_KEYS PHP_SHA256=$PHP72_TAR_SHA256 /php/build.sh
