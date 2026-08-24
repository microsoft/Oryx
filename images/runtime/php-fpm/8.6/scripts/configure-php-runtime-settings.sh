#!/usr/bin/env bash
# Writes the App Service PHP runtime defaults that differ from upstream PHP. It enables the
# recommended OPcache capacity and revalidation behavior, sends PHP errors to container stderr,
# disables browser-facing error output, and standardizes the runtime timezone on UTC.

set -euxo pipefail

cat > "$PHP_INI_DIR/conf.d/opcache-recommended.ini" <<'EOF'
opcache.memory_consumption=128
opcache.interned_strings_buffer=8
opcache.max_accelerated_files=4000
opcache.revalidate_freq=60
opcache.enable_cli=1
EOF

cat > "$PHP_INI_DIR/conf.d/php.ini" <<'EOF'
error_log=/dev/stderr
display_errors=Off
log_errors=On
display_startup_errors=Off
date.timezone=UTC
EOF
