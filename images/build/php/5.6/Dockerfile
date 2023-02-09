# Build PHP 5.6
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php56-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$FINAL_STRETCH_PHP56_VERSION GPG_KEYS=$FINAL_STRETCH_PHP56_KEYS PHP_SHA256=$FINAL_STRETCH_PHP56_TAR_SHA256 /php/build.sh