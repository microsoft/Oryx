#!/usr/bin/env bash
# Downloads the PHP preview source archive used by the runtime image and verifies both its
# pinned SHA256 digest and PHP release signature. Build-only GPG packages are removed after
# verification so they do not become accidental runtime dependencies.

set -euxo pipefail

savedAptMark="$(apt-mark showmanual)"

apt-get update
apt-get install -y --no-install-recommends gnupg dirmngr
rm -rf /var/lib/apt/lists/*

mkdir -p /usr/src
cd /usr/src

phpUrl="https://downloads.php.net/~svpernova09/php-${PHP_VERSION}.tar.xz"
phpSignatureUrl="${phpUrl}.asc"

curl -fsSL --retry 5 --retry-delay 5 --retry-max-time 120 -o php.tar.xz "$phpUrl"
echo "$PHP_SHA256 *php.tar.xz" | sha256sum -c -

curl -fsSL --retry 5 --retry-delay 5 --retry-max-time 120 \
    -o php.tar.xz.asc \
    "$phpSignatureUrl"

export GNUPGHOME
GNUPGHOME="$(mktemp -d)"
"${IMAGES_DIR}/receiveGpgKeys.sh" $GPG_KEYS
gpg --batch --verify php.tar.xz.asc php.tar.xz
gpgconf --kill all
rm -rf "$GNUPGHOME"

apt-mark auto '.*' > /dev/null
apt-mark manual $savedAptMark > /dev/null
apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false
