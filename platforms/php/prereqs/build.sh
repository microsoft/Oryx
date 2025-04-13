#!/bin/bash

set -eux

PHP_MAJOR=${PHP_VERSION:0:1}
PHP_MINOR=${PHP_VERSION:2:1}
INSTALLATION_BASE_DIR="/opt/php/"
INSTALLATION_PREFIX="$INSTALLATION_BASE_DIR$PHP_VERSION"
PHP_INI_DIR="$INSTALLATION_PREFIX/ini"
PHP_SRC_DIR="/usr/src/php"

mkdir -p "$PHP_INI_DIR/conf.d";

# Apply stack smash protection to functions using local buffers and alloca()
# Make PHP's main executable position-independent (improves ASLR security mechanism, and has no performance impact on x86_64)
# Enable optimization (-O2)
# Enable linker optimization (this sorts the hash buckets to improve cache locality, and is non-default)
# Adds GNU HASH segments to generated executables (this is used if present, and is much faster than sysv hash; in this configuration, sysv hash is also generated)
# https://github.com/docker-library/php/issues/272
PHP_CFLAGS="-fstack-protector-strong -fpic -fpie -O2"
PHP_CPPFLAGS="$PHP_CFLAGS"
PHP_LDFLAGS="-Wl,-O1 -Wl,--hash-style=both -pie"

PHP_URL="https://secure.php.net/get/php-$PHP_VERSION.tar.xz/from/this/mirror"
PHP_ASC_URL="" # "https://secure.php.net/get/php-$PHP_VERSION.tar.xz.asc/from/this/mirror"
GPG_KEYS=($GPG_KEYS) # Cast the string to an array
PHP_MD5=""

debianFlavor=$DEBIAN_FLAVOR
phpSdkFileName=""

if [ "$debianFlavor" == "stretch" ]; then
	# Use default php sdk file name
	phpSdkFileName=php-$PHP_VERSION.tar.gz
else
	phpSdkFileName=php-$debianFlavor-$PHP_VERSION.tar.gz
	# for buster and ubuntu we would need following libraries to build php 
	apt-get update && \
	apt-get upgrade -y && \
	DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
		libssl-dev \
		libncurses5-dev \
		libsqlite3-dev \
		libreadline-dev \
		libgdm-dev \
		libdb4o-cil-dev \
		libpcap-dev \
		libxml2 \
		libxml2-dev
fi

fetchDeps='wget';
if ! command -v gpg > /dev/null; then
	fetchDeps="$fetchDeps dirmngr gnupg"
fi

apt-get update && apt-get upgrade -y && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends build-essential $fetchDeps
rm -rf /var/lib/apt/lists/*

mkdir -p /usr/src
cd /usr/src
wget -O php.tar.xz "$PHP_URL"

if [ -n "$PHP_SHA256" ]; then
	echo "$PHP_SHA256 *php.tar.xz" | sha256sum -c -;
fi;
if [ -n "$PHP_MD5" ]; then
	echo "$PHP_MD5 *php.tar.xz" | md5sum -c -;
fi;

if [ -n "$PHP_ASC_URL" ]; then
	wget -O php.tar.xz.asc "$PHP_ASC_URL";
	export GNUPGHOME="$(mktemp -d)";
	/tmp/receiveGpgKeys.sh ${GPG_KEYS[0]} ${GPG_KEYS[1]}
	gpg --batch --verify php.tar.xz.asc php.tar.xz;
	command -v gpgconf > /dev/null && gpgconf --kill all;
	rm -rf "$GNUPGHOME";
fi;

apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false $fetchDeps

versionDevReqs='libssl-dev'
if [ $PHP_MAJOR == '5' ]; then
	versionDevReqs='libssl1.0-dev'
fi

if [[ $PHP_VERSION == 7.4.* || $PHP_VERSION == 7.3.* || $PHP_VERSION == 7.2.* ]]; then 
	apt-get update
	apt-get install -y --no-install-recommends libargon2-dev libonig-dev
fi	

if [ $PHP_MAJOR == '8' ]; then
	apt-get update
	apt-get install -y --no-install-recommends libargon2-dev libonig-dev libsodium-dev
fi

savedAptMark="$(apt-mark showmanual)";
apt-get update;
apt-get upgrade -y;
DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
	libcurl4-openssl-dev \
	libedit-dev \
	libsqlite3-dev \
	libxml2-dev \
	zlib1g-dev \
	$versionDevReqs;

rm -rf /var/lib/apt/lists/*;

export \
	CFLAGS="$PHP_CFLAGS" \
	CPPFLAGS="$PHP_CPPFLAGS" \
	LDFLAGS="$PHP_LDFLAGS"

/php/docker-php-source.sh extract;
cd $PHP_SRC_DIR;
gnuArch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)";
debMultiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)";

# Only needed for php 7.1 and also the following has an error. It checks for /usr/include/curl and links to /usr/local/include/curl.
# In a way it never worked!!
# # https://bugs.php.net/bug.php?id=74125
# if [ ! -d /usr/include/curl ]; then
# 	ln -sT "/usr/include/$debMultiarch/curl" /usr/local/include/curl;
# fi;

versionConfigureArgs=''
# in PHP 7.4+, the pecl/pear installers are officially deprecated (requiring an explicit "--with-pear") and will be removed in PHP 8+; 
# see also https://github.com/docker-library/php/issues/846#issuecomment-505638494
if [[ $PHP_VERSION == 7.4.* || $PHP_VERSION == 8.0.* ]]; then
	versionConfigureArgs='--with-password-argon2 --with-sodium=shared --with-pear'
else
	versionConfigureArgs='--with-password-argon2 --with-sodium=shared'
fi

./configure \
		--build="$gnuArch" \
		--prefix="$INSTALLATION_PREFIX" \
		--with-config-file-path="$PHP_INI_DIR" \
		--with-config-file-scan-dir="$PHP_INI_DIR/conf.d" \
		--enable-option-checking=fatal \
		--with-mhash \
		--enable-ftp \
		--enable-mbstring \
		--enable-mysqlnd \
		--enable-bcmath \
		$versionConfigureArgs \
		--with-curl \
		--with-libedit \
		--with-openssl \
		--with-zlib \
		$(test "$gnuArch" = 's390x-linux-gnu' && echo '--without-pcre-jit') \
		--with-libdir="lib/$debMultiarch";

make -j "$(nproc)";
find -type f -name '*.a' -delete;
make install;
find $INSTALLATION_PREFIX -type f -executable -exec strip --strip-all '{}' + || true;
make clean;

# https://github.com/docker-library/php/issues/692 (copy default example "php.ini" files somewhere easily discoverable)
cp -v php.ini-* "$PHP_INI_DIR/";
cp "$PHP_INI_DIR/php.ini-production" "$PHP_INI_DIR/php.ini"

cd /;
/php/docker-php-source.sh delete;

# reset apt-mark's "manual" list so that "purge --auto-remove" will remove all build dependencies
apt-mark auto '.*' > /dev/null;
[ -z "$savedAptMark" ] || apt-mark manual $savedAptMark;
find $INSTALLATION_PREFIX -type f -executable -exec ldd '{}' ';' \
	| awk '/=>/ { print $(NF-1) }' \
	| sort -u \
	| xargs -r dpkg-query --search \
	| cut -d: -f1 \
	| sort -u \
	| xargs -r apt-mark manual \

apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false;

$INSTALLATION_PREFIX/bin/php --version;

# https://github.com/docker-library/php/issues/443
# in PHP 7.4+, the pecl/pear installers are officially deprecated (requiring an explicit "--with-pear") and will be removed in PHP 8+; 
# see also https://github.com/docker-library/php/issues/846#issuecomment-505638494
if [[ $PHP_VERSION == 7.2.* || $PHP_VERSION == 7.3.* || $PHP_VERSION == 7.4.* || $PHP_VERSION == 8.0.* ]]; then
	$INSTALLATION_PREFIX/bin/pecl update-channels;
	rm -rf /tmp/pear ~/.pearrc
fi

if [ $PHP_MAJOR == '7' ] && [ $PHP_MINOR != '0' ]; then
	PHP_INI_DIR=$PHP_INI_DIR php=$INSTALLATION_PREFIX/bin/php /php/docker-php-ext-enable.sh sodium
fi

if [[ $PHP_VERSION == 7.2.* || $PHP_VERSION == 7.3.* || $PHP_VERSION == 7.4.* || $PHP_VERSION == 8.0.* ]]; then \
        echo "pecl/mysqlnd_azure requires PHP (version >= 7.2.*, version <= 7.99.99)"; \
        pecl install mysqlnd_azure \
        && /php/docker-php-ext-enable.sh mysqlnd_azure; \
fi

compressedSdkDir="/tmp/compressedSdk/php"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$phpSdkFileName .