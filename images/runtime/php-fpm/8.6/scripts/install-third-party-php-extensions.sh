#!/usr/bin/env bash
# Builds third-party extensions that have been explicitly validated with PHP 8.6. Redis is pinned
# to a compatible upstream commit until a stable PECL release supports PHP 8.6. Other extensions
# match the PHP 8.5 runtime, with small patches for PHP 8.6's removed aliases and stream-error API.

set -euxo pipefail

redisCommit='777f7377674a8f5d23a7ba739d165daeebc13e47'
mongodbVersion='2.5.3'
sqlsrvVersion='5.13.3'
imapVersion='1.0.3'
extensionBuildDir='/tmp/php-extensions'

mkdir -p "$extensionBuildDir"

curl -fsSL --retry 5 \
    -o "$extensionBuildDir/redis.tar.gz" \
    "https://github.com/phpredis/phpredis/archive/${redisCommit}.tar.gz"
tar -xzf "$extensionBuildDir/redis.tar.gz" -C "$extensionBuildDir"

cd "$extensionBuildDir/phpredis-${redisCommit}"
phpize
./configure --enable-option-checking=fatal
make -j "$(nproc)"
make install
docker-php-ext-enable --ini-name 20-redis.ini redis

for extension in mongodb sqlsrv pdo_sqlsrv imap; do
    case "$extension" in
        mongodb) version="$mongodbVersion" ;;
        sqlsrv|pdo_sqlsrv) version="$sqlsrvVersion" ;;
        imap) version="$imapVersion" ;;
    esac

    curl -fsSL --retry 5 \
        -o "$extensionBuildDir/$extension.tgz" \
        "https://pecl.php.net/get/${extension}-${version}.tgz"
    tar -xzf "$extensionBuildDir/$extension.tgz" -C "$extensionBuildDir"
    cd "$extensionBuildDir/${extension}-${version}"

    case "$extension" in
        sqlsrv)
            patch --fuzz=0 -p1 < /opt/oryx/php/sqlsrv-php86.patch
            patch --fuzz=0 -p1 < /opt/oryx/php/sqlsrv-stream-php86.patch
            ;;
        pdo_sqlsrv)
            patch --fuzz=0 -p1 < /opt/oryx/php/sqlsrv-stream-php86.patch
            ;;
        imap)
            patch --fuzz=0 -p1 < /opt/oryx/php/imap-php86.patch
            ;;
    esac

    phpize
    if [ "$extension" = imap ]; then
        ./configure --enable-option-checking=fatal --with-kerberos --with-imap-ssl
    else
        ./configure --enable-option-checking=fatal
    fi
    make -j "$(nproc)"
    make install

    case "$extension" in
        mongodb) docker-php-ext-enable --ini-name 20-mongodb.ini mongodb ;;
        sqlsrv) docker-php-ext-enable --ini-name 20-sqlsrv.ini sqlsrv ;;
        pdo_sqlsrv) docker-php-ext-enable --ini-name 30-pdo_sqlsrv.ini pdo_sqlsrv ;;
        # PHP 8.5 installs IMAP without enabling it by default.
        imap) php -d extension=imap --ri imap ;;
    esac
done

cd /
rm -rf "$extensionBuildDir"

php -m | grep -Fx redis
php -m | grep -Fx mongodb
php -m | grep -Fx sqlsrv
php -m | grep -Fx pdo_sqlsrv
