# Build PHP 7.2
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php72-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$FINAL_STRETCH_PHP72_VERSION GPG_KEYS=$FINAL_STRETCH_PHP72_KEYS PHP_SHA256=$FINAL_STRETCH_PHP72_TAR_SHA256 /php/build.sh
