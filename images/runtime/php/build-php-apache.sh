#!/bin/bash
set -euo pipefail

: "${PHP_VERSION:?PHP_VERSION is required}"
: "${PHP_SHA256:?PHP_SHA256 is required, including for mirrored source}"
: "${PHP_INI_DIR:?PHP_INI_DIR is required}"

. /etc/os-release
case "$VERSION_CODENAME" in
    bullseye)
        odbc_version=2.3.7
        msodbcsql18_version=18.1.2.1-1
        ;;
    bookworm)
        odbc_version=2.3.11-2+deb12u1
        msodbcsql18_version=18.3.3.1-1
        ;;
    *) echo "Unsupported Apache PHP distribution: $VERSION_CODENAME" >&2; exit 1 ;;
esac
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
    | gpg --batch --yes --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/${VERSION_ID}/prod ${VERSION_CODENAME} main" \
    > /etc/apt/sources.list.d/mssql-release.list
apt-get update
ACCEPT_EULA=Y apt-get install -y --no-install-recommends \
    msodbcsql17=17.10.6.1-1 "msodbcsql18=$msodbcsql18_version" \
    "odbcinst1debian2=$odbc_version" "odbcinst=$odbc_version" \
    "unixodbc=$odbc_version" "unixodbc-dev=$odbc_version"
apt-mark hold msodbcsql17 msodbcsql18 odbcinst1debian2 odbcinst unixodbc unixodbc-dev

mkdir -p /usr/src "$PHP_INI_DIR/conf.d"
cd /usr/src
source_url="${PHP_SOURCE_BASE_URL:-https://www.php.net/distributions}"
curl -fsSL -o php.tar.xz "${source_url%/}/php-${PHP_VERSION}.tar.xz"
echo "$PHP_SHA256 *php.tar.xz" | sha256sum -c -
if [ -z "${PHP_SOURCE_BASE_URL:-}" ]; then
    curl -fsSL -o php.tar.xz.asc "${source_url}/php-${PHP_VERSION}.tar.xz.asc"
    export GNUPGHOME
    GNUPGHOME="$(mktemp -d)"
    /tmp/oryx/images/receiveGpgKeys.sh $GPG_KEYS
    gpg --batch --verify php.tar.xz.asc php.tar.xz
    gpgconf --kill all
    rm -rf "$GNUPGHOME"
    unset GNUPGHOME
fi

export CFLAGS="$PHP_CFLAGS" CPPFLAGS="$PHP_CPPFLAGS" LDFLAGS="$PHP_LDFLAGS"
docker-php-source extract
cd /usr/src/php
gnu_arch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)"
multiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)"
if [ ! -d /usr/include/curl ] && [ ! -e /usr/local/include/curl ]; then
    ln -sT "/usr/include/$multiarch/curl" /usr/local/include/curl
fi
configure_args=()
if [ "$gnu_arch" = s390x-linux-gnu ]; then
    configure_args+=(--without-pcre-jit)
fi
./configure \
    --build="$gnu_arch" \
    --with-config-file-path="$PHP_INI_DIR" \
    --with-config-file-scan-dir="$PHP_INI_DIR/conf.d" \
    --enable-option-checking=fatal \
    --with-mhash --enable-ftp --enable-mbstring --enable-mysqlnd \
    --with-password-argon2 --with-sodium=shared \
    --with-pdo-sqlite=/usr --with-sqlite3=/usr \
    --with-curl --with-libedit --with-openssl --with-zlib --with-pear \
    --with-libdir="lib/$multiarch" \
    --with-apxs2 --disable-cgi ac_cv_func_mmap=no "${configure_args[@]}"
make -j "$(nproc)"
make install
cp php.ini-* "$PHP_INI_DIR/"
make clean
cd /
docker-php-source delete
docker-php-ext-enable sodium
rm -f /usr/src/php.tar.xz.asc
php --version
