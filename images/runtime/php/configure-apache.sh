#!/bin/bash
set -euo pipefail

export APACHE_CONFDIR=/etc/apache2
sed -ri 's/^export ([^=]+)=(.*)$/: ${\1:=\2}\nexport \1/' /etc/apache2/envvars
. /etc/apache2/envvars
for dir in "$APACHE_LOCK_DIR" "$APACHE_RUN_DIR" "$APACHE_LOG_DIR"; do
    mkdir -p "$dir"
    chown "$APACHE_RUN_USER:$APACHE_RUN_GROUP" "$dir"
    chmod 777 "$dir"
done
ln -sfT /dev/stderr "$APACHE_LOG_DIR/error.log"
ln -sfT /dev/stdout "$APACHE_LOG_DIR/access.log"
ln -sfT /dev/stdout "$APACHE_LOG_DIR/other_vhosts_access.log"
chown -R --no-dereference "$APACHE_RUN_USER:$APACHE_RUN_GROUP" "$APACHE_LOG_DIR"
rm -f /var/www/html/index.html
mkdir -p /var/www/html /home/site/wwwroot
chown www-data:www-data /var/www/html /home/site/wwwroot
chmod 777 /var/www/html
a2dismod mpm_event
a2enmod mpm_prefork rewrite expires include deflate remoteip headers
cat > /etc/apache2/conf-available/docker-php.conf <<'CONF'
<FilesMatch "\.(?i:php[0-9]*|phtml?)$">
    SetHandler application/x-httpd-php
</FilesMatch>
DirectoryIndex disabled
DirectoryIndex index.php index.html
<Directory "${APACHE_DOCUMENT_ROOT}">
    Options -Indexes
    AllowOverride All
    Require all granted
</Directory>
ServerSignature Off
ServerTokens Prod
<DirectoryMatch "^/.*/\.git/">
    Require all denied
</DirectoryMatch>
CONF
a2enconf docker-php
sed -ri 's!/var/www/html!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/sites-available/*.conf
sed -ri 's!/var/www/!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/apache2.conf
sed -ri 's!<VirtualHost \*:80>!<VirtualHost *:${APACHE_PORT}>!g' /etc/apache2/sites-available/*.conf
sed -ri 's!<VirtualHost _default_:443>!<VirtualHost _default_:${APACHE_PORT}>!g' /etc/apache2/sites-available/*.conf
sed -ri 's!Listen 80!Listen ${APACHE_PORT}!g' /etc/apache2/ports.conf
cat > "$PHP_INI_DIR/conf.d/opcache-recommended.ini" <<'INI'
opcache.memory_consumption=128
opcache.interned_strings_buffer=8
opcache.max_accelerated_files=4000
opcache.revalidate_freq=60
opcache.enable_cli=1
INI
cat > "$PHP_INI_DIR/conf.d/php.ini" <<'INI'
error_log=/var/log/apache2/php-error.log
display_errors=Off
log_errors=On
display_startup_errors=Off
date.timezone=UTC
INI
APACHE_DOCUMENT_ROOT=/home/site/wwwroot APACHE_PORT=8080 apache2 -t
