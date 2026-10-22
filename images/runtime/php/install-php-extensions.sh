#!/bin/bash
set -euo pipefail

export CFLAGS="$PHP_CFLAGS" CPPFLAGS="$PHP_CPPFLAGS" LDFLAGS="$PHP_LDFLAGS"
multiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)"
ln -sf "/usr/lib/$multiarch/libldap.so" /usr/lib/libldap.so
ln -sf "/usr/lib/$multiarch/liblber.so" /usr/lib/liblber.so
ln -sf "/usr/include/$multiarch/gmp.h" /usr/include/gmp.h
docker-php-ext-configure gd --with-freetype --with-jpeg
PHP_OPENSSL=yes docker-php-ext-configure imap --with-kerberos --with-imap-ssl
docker-php-ext-configure pdo_odbc --with-pdo-odbc=unixODBC,/usr
docker-php-ext-install -j"$(nproc)" \
    gd mysqli opcache pdo pdo_mysql pdo_pgsql pgsql ldap intl gmp zip \
    bcmath mbstring pcntl calendar exif gettext imap tidy shmop soap \
    sockets sysvmsg sysvsem sysvshm pdo_odbc xsl
pecl update-channels
pecl install redis mongodb
docker-php-ext-enable redis mongodb
case "$PHP_VERSION" in
    8.2.*)
        pecl install imagick sqlsrv-5.12.0 pdo_sqlsrv-5.12.0
        docker-php-ext-enable imagick
        ;;
    8.3.*)
        # Retain the PHP 8.3 Imagick exclusion used by the FPM images.
        pecl install sqlsrv pdo_sqlsrv
        ;;
    *) echo "Unsupported Apache PHP version: $PHP_VERSION" >&2; exit 1 ;;
esac
docker-php-ext-enable --ini-name 20-sqlsrv.ini sqlsrv
docker-php-ext-enable --ini-name 30-pdo_sqlsrv.ini pdo_sqlsrv

docker-php-source extract
cd /usr/src/php/ext/odbc
phpize
sed -ri 's@^ *test +"\$PHP_.*" *= *"no" *&& *PHP_.*=yes *$@#&@g' configure
./configure --with-unixODBC=shared,/usr
docker-php-ext-install odbc
cd /
docker-php-source delete
rm -rf /tmp/pear
rm -f /root/.pearrc
php -m
