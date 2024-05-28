# Build PHP 7.0
FROM oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs AS php70-build
RUN set -eux && . /php/__phpVersions.sh && PHP_VERSION=$PHP70_VERSION GPG_KEYS=$PHP70_KEYS PHP_SHA256=$PHP70_TAR_SHA256 /php/build.sh
