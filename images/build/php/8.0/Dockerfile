# Build PHP 8.0
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php80-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP80_VERSION GPG_KEYS=$PHP80_KEYS PHP_SHA256=$PHP80_TAR_SHA256 /php/build.sh