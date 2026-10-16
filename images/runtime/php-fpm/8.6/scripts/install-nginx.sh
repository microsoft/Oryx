#!/usr/bin/env bash
# Installs Nginx from the official nginx.org Ubuntu Resolute repository. The repository signing
# key is fingerprint-checked before use, the vendor default server is removed, and compatibility
# directories expected by App Service customer configurations are created.

set -euxo pipefail

nginxKeyring='/usr/share/keyrings/nginx-archive-keyring.gpg'
nginxKeyFingerprint='573BFD6B3D8FBC641079A6ABABF5BD827BD9BF62'

apt-get update
apt-get install -y --no-install-recommends curl gnupg2 nano

curl -fsSL https://nginx.org/keys/nginx_signing.key \
    | gpg --dearmor -o "$nginxKeyring"

gpg --no-default-keyring \
    --keyring "$nginxKeyring" \
    --list-keys "$nginxKeyFingerprint"

echo "deb [signed-by=${nginxKeyring}] https://nginx.org/packages/ubuntu resolute nginx" \
    > /etc/apt/sources.list.d/nginx.list

apt-get update
apt-get install -y --no-install-recommends nginx

rm -rf /var/lib/apt/lists/* /etc/nginx/conf.d/default.conf

mkdir -p \
    /etc/nginx/sites-available \
    /etc/nginx/sites-enabled \
    /etc/nginx/snippets \
    /etc/nginx/modules-enabled \
    /etc/nginx/modules-available
