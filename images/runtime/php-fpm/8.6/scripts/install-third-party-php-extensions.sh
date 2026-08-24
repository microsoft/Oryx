#!/usr/bin/env bash
# Builds third-party extensions that have been explicitly validated with PHP 8.6. Redis is pinned
# to a compatible upstream commit until a stable PECL release supports PHP 8.6; MongoDB uses its
# compatible PECL release. IMAP and SQLSRV are intentionally excluded because they do not compile.

set -euxo pipefail

redisCommit='777f7377674a8f5d23a7ba739d165daeebc13e47'
mongodbVersion='2.4.0'
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

curl -fsSL --retry 5 \
    -o "$extensionBuildDir/mongodb.tgz" \
    "https://pecl.php.net/get/mongodb-${mongodbVersion}.tgz"
tar -xzf "$extensionBuildDir/mongodb.tgz" -C "$extensionBuildDir"

cd "$extensionBuildDir/mongodb-${mongodbVersion}"
phpize
./configure --enable-option-checking=fatal
make -j "$(nproc)"
make install
docker-php-ext-enable --ini-name 20-mongodb.ini mongodb

rm -rf "$extensionBuildDir"

php -m | grep -Fx redis
php -m | grep -Fx mongodb
