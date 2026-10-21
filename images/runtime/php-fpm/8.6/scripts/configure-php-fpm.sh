#!/usr/bin/env bash
# Creates the container-oriented PHP-FPM configuration. It keeps upstream defaults discoverable
# while routing worker and access logs to container stderr, exposing FastCGI on port 9000, and
# disabling daemon mode so PHP-FPM can operate correctly as the container process.

set -euxo pipefail

phpEtcDir="${PHP_INI_DIR%/php}"
cd "$phpEtcDir"

cp -v php-fpm.conf.default php-fpm.conf
cp -v php-fpm.d/www.conf.default php-fpm.d/www.conf

grep -E '^listen = 127.0.0.1:9000' php-fpm.d/www.conf
sed -ri 's/^(listen = 127.0.0.1:9000)/;\1/' php-fpm.d/www.conf

cat > php-fpm.d/docker.conf <<'EOF'
[global]
error_log = /proc/self/fd/2
log_limit = 8192

[www]
access.log = /proc/self/fd/2
clear_env = no
catch_workers_output = yes
decorate_workers_output = no
listen = 9000
EOF

cat > php-fpm.d/zz-docker.conf <<'EOF'
[global]
daemonize = no
EOF

cat > "$PHP_INI_DIR/conf.d/docker-fpm.ini" <<'EOF'
fastcgi.logging = Off
EOF
