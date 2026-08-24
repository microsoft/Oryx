#!/usr/bin/env bash
# Compiles PHP-FPM from the previously verified source archive using the PHP 8.6 upstream
# configuration baseline. It retains only libraries required by installed PHP binaries and
# removes compilers, headers, extracted sources, and other transient build dependencies.

set -euxo pipefail

savedAptMark="$(apt-mark showmanual)"

export CFLAGS="$PHP_CFLAGS"
export CPPFLAGS="$PHP_CPPFLAGS"
export LDFLAGS="$PHP_LDFLAGS"
export PHP_BUILD_PROVIDER='https://github.com/microsoft/Oryx'
export PHP_UNAME='Linux - Oryx'

docker-php-source extract
cd /usr/src/php

gnuArch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)"
debMultiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)"

if [ ! -d /usr/include/curl ]; then
    ln -sT "/usr/include/$debMultiarch/curl" /usr/local/include/curl
fi

test "$PHP_INI_DIR" != "${PHP_INI_DIR%/php}"

./configure \
    --build="$gnuArch" \
    --sysconfdir="${PHP_INI_DIR%/php}" \
    --with-config-file-path="$PHP_INI_DIR" \
    --with-config-file-scan-dir="$PHP_INI_DIR/conf.d" \
    --enable-option-checking=fatal \
    --with-mhash \
    --enable-pic \
    --enable-mysqlnd \
    --with-password-argon2 \
    --with-sodium=shared \
    --with-pdo-sqlite=/usr \
    --with-sqlite3=/usr \
    --with-curl \
    --with-iconv \
    --with-openssl \
    --with-readline \
    --with-zlib \
    --disable-phpdbg \
    --with-libdir="lib/$debMultiarch" \
    --disable-cgi \
    --enable-fpm \
    --with-fpm-user=www-data \
    --with-fpm-group=www-data

make -j "$(nproc)"
find -type f -name '*.a' -delete
make install

find /usr/local -type f -perm '/0111' \
    -exec sh -euxc 'strip --strip-all "$@" || :' -- '{}' +

make clean
cp -v php.ini-* "$PHP_INI_DIR/"

cd /
docker-php-source delete

apt-mark auto '.*' > /dev/null
[ -z "$savedAptMark" ] || apt-mark manual $savedAptMark

find /usr/local -type f -executable -exec ldd '{}' ';' \
    | awk '/=>/ { so = $(NF-1); if (index(so, "/usr/local/") == 1) { next }; gsub("^/(usr/)?", "", so); printf "*%s\n", so }' \
    | sort -u \
    | xargs -rt dpkg-query --search \
    | awk 'sub(":$", "", $1) { print $1 }' \
    | sort -u \
    | xargs -r apt-mark manual

apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false
rm -rf /var/lib/apt/lists/*

php --version
