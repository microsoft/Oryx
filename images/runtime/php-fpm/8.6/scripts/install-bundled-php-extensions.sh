#!/usr/bin/env bash
# Installs the PHP extensions that ship with the PHP source tree and are part of the App Service
# runtime contract. It configures architecture-neutral library paths, enables TLS for the shared
# FTP extension, and compiles database, image, internationalization, and process extensions.
# Native ODBC uses the same generated-configure workaround as the existing PHP runtime images.

set -euxo pipefail

debMultiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)"
ln -sf "/usr/lib/$debMultiarch/libldap.so" /usr/lib/libldap.so
ln -sf "/usr/lib/$debMultiarch/liblber.so" /usr/lib/liblber.so
ln -sf "/usr/include/$debMultiarch/gmp.h" /usr/include/gmp.h

docker-php-ext-configure gd --with-freetype --with-jpeg
docker-php-ext-configure ftp --with-ftp-ssl
docker-php-ext-configure pdo_odbc --with-pdo-odbc=unixODBC,/usr

docker-php-ext-install -j "$(nproc)" \
    bcmath \
    calendar \
    exif \
    ftp \
    gd \
    gettext \
    gmp \
    intl \
    ldap \
    mbstring \
    mysqli \
    pcntl \
    pdo_mysql \
    pdo_odbc \
    pdo_pgsql \
    pgsql \
    shmop \
    soap \
    sockets \
    sysvmsg \
    sysvsem \
    sysvshm \
    tidy \
    xsl \
    zip

docker-php-source extract
cd /usr/src/php/ext/odbc
phpize
sed -ri 's@^ *test +"\$PHP_.*" *= *"no" *&& *PHP_.*=yes *$@#&@g' configure
./configure --with-unixODBC=shared,/usr
make -j "$(nproc)"
make install
docker-php-ext-enable odbc
cd /
docker-php-source delete

php --ri ftp | grep -F 'FTPS support => enabled'
php -m | grep -Fx odbc
