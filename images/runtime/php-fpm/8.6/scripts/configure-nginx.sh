#!/usr/bin/env bash
# Adapts the nginx.org package to the established App Service PHP configuration contract. It uses
# www-data, restores sites-enabled and compatibility parameters/files, preserves historical MIME
# mappings, configures production worker behavior, and validates the final Nginx configuration.

set -euxo pipefail

ln -s /etc/nginx/sites-available/default /etc/nginx/sites-enabled/default

sed -ri -e 's!^user\s+\S+;!user  www-data;!' /etc/nginx/nginx.conf
sed -ri -e 's!worker_connections\s+1024!worker_connections  10068!g' /etc/nginx/nginx.conf
sed -ri -e '/worker_connections/a\    multi_accept  on;' /etc/nginx/nginx.conf
sed -ri -e 's!#tcp_nopush\s+on;!tcp_nopush     on;!' /etc/nginx/nginx.conf
sed -ri -e 's!#gzip\s+on;!gzip  on;!' /etc/nginx/nginx.conf
sed -ri -e '/include\s+.*mime\.types;/a\    types_hash_max_size  2048;' /etc/nginx/nginx.conf
sed -ri -e '/include\s+.*conf\.d\/\*\.conf;/a\    include /etc/nginx/sites-enabled/*;' /etc/nginx/nginx.conf

sed -i '/video\/x-msvideo/a\    video/ogg                                        ogv;\n    video/x-matroska                                 mkv;' \
    /etc/nginx/mime.types
sed -i '/REMOTE_PORT/a\fastcgi_param  REMOTE_USER        $remote_user;' \
    /etc/nginx/fastcgi_params

chown -R www-data:www-data /var/cache/nginx
nginx -t
