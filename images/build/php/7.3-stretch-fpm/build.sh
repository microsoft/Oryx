#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

PHP_VERSION='7.3.2'
PHP_INI_DIR="/opt/php/ini/$PHP_VERSION"
PHP_SRC_DIR="/usr/src/php"
INSTALLATION_PREFIX="/opt/php/$PHP_VERSION"

# prevent Debian's PHP packages from being installed
# https://github.com/docker-library/php/pull/542
{
	echo 'Package: php*';
	echo 'Pin: release *';
	echo 'Pin-Priority: -1';
} > /etc/apt/preferences.d/no-debian-php

# dependencies required for running "phpize"
# (see persistent deps below)
PHPIZE_DEPS=(autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c)

# persistent / runtime deps
apt-get update && apt-get install -y \
		$PHPIZE_DEPS \
		ca-certificates \
		curl \
		xz-utils \
	--no-install-recommends && rm -r /var/lib/apt/lists/*

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

GPG_KEYS=(CBAF69F173A0FEA4B537F470D66C9593118BCCB6 F38252826ACD957EF380D39F2F7956BC5DA04B5D)

PHP_URL="https://secure.php.net/get/php-$PHP_VERSION.tar.xz/from/this/mirror"
PHP_ASC_URL="" # "https://secure.php.net/get/php-$PHP_VERSION.tar.xz.asc/from/this/mirror"
PHP_SHA256="010b868b4456644ae227d05ad236c8b0a1f57dc6320e7e5ad75e86c5baf0a9a8"
PHP_MD5=""


fetchDeps='
	wget
';
if ! command -v gpg > /dev/null; then
	fetchDeps="$fetchDeps
		dirmngr
		gnupg
	";
fi;
apt-get update;
apt-get install -y --no-install-recommends $fetchDeps;
rm -rf /var/lib/apt/lists/*;

mkdir -p /usr/src;
cd /usr/src;

wget -O php.tar.xz "$PHP_URL";

if [ -n "$PHP_SHA256" ]; then
	echo "$PHP_SHA256 *php.tar.xz" | sha256sum -c -;
fi;
if [ -n "$PHP_MD5" ]; then
	echo "$PHP_MD5 *php.tar.xz" | md5sum -c -;
fi;

if [ -n "$PHP_ASC_URL" ]; then
	wget -O php.tar.xz.asc "$PHP_ASC_URL";
	export GNUPGHOME="$(mktemp -d)";
	for key in $GPG_KEYS; do
		gpg --batch --keyserver ha.pool.sks-keyservers.net --recv-keys "$key";
	done;
	gpg --batch --verify php.tar.xz.asc php.tar.xz;
	command -v gpgconf > /dev/null && gpgconf --kill all;
	rm -rf "$GNUPGHOME";
fi;

apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false $fetchDeps


savedAptMark="$(apt-mark showmanual)";
apt-get update;
apt-get install -y --no-install-recommends \
	libcurl4-openssl-dev \
	libedit-dev \
	libsodium-dev \
	libsqlite3-dev \
	libssl-dev \
	libxml2-dev \
	zlib1g-dev \
	${PHP_EXTRA_BUILD_DEPS:-}

##<argon2>##
sed -e 's/stretch/buster/g' /etc/apt/sources.list > /etc/apt/sources.list.d/buster.list;
{
	echo 'Package: *';
	echo 'Pin: release n=buster';
	echo 'Pin-Priority: -10';
	echo;
	echo 'Package: libargon2*';
	echo 'Pin: release n=buster';
	echo 'Pin-Priority: 990';
} > /etc/apt/preferences.d/argon2-buster;
apt-get update;
apt-get install -y --no-install-recommends libargon2-dev;
##</argon2>##
rm -rf /var/lib/apt/lists/*;

export \
	CFLAGS="$PHP_CFLAGS" \
	CPPFLAGS="$PHP_CPPFLAGS" \
	LDFLAGS="$PHP_LDFLAGS"

/php/docker-php-source.sh extract;
cd $PHP_SRC_DIR;
gnuArch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)";
debMultiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)";
# https://bugs.php.net/bug.php?id=74125
if [ ! -d /usr/include/curl ]; then
	ln -sT "/usr/include/$debMultiarch/curl" /usr/local/include/curl;
fi;

# --enable-option-checking: make sure invalid --configure-flags are fatal errors intead of just warnings
# --with-mhash: https://github.com/docker-library/php/issues/439
#
#
#

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
		--with-password-argon2 \
		--with-sodium=shared \
		--with-curl \
		--with-libedit \
		--with-openssl \
		--with-zlib \
		$(test "$gnuArch" = 's390x-linux-gnu' && echo '--without-pcre-jit') \
		--with-libdir="lib/$debMultiarch" \
		${PHP_EXTRA_CONFIGURE_ARGS:-}

make -j "$(nproc)";
find -type f -name '*.a' -delete;
make install;
find $INSTALLATION_PREFIX -type f -executable -exec strip --strip-all '{}' + || true;
make clean;

# https://github.com/docker-library/php/issues/692 (copy default example "php.ini" files somewhere easily discoverable)
cp -v php.ini-* "$PHP_INI_DIR/";

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
$INSTALLATION_PREFIX/bin/pecl update-channels;
rm -rf /tmp/pear ~/.pearrc

# TODO: install Composer, FPM

# # sodium was built as a shared module (so that it can be replaced later if so desired), so let's enable it too (https://github.com/docker-library/php/issues/598)
# RUN /php/docker-php-ext-enable sodium

# ENTRYPOINT ["./docker-php-entrypoint"]
# ##<autogenerated>##
# WORKDIR /var/www/html

# RUN set -ex
# 	&& cd /usr/local/etc 
# 	&& if [ -d php-fpm.d ]; then \
# 		# for some reason, upstream's php-fpm.conf.default has "include=NONE/etc/php-fpm.d/*.conf"
# 		sed 's!=NONE/!=!g' php-fpm.conf.default | tee php-fpm.conf > /dev/null; \
# 		cp php-fpm.d/www.conf.default php-fpm.d/www.conf; \
# 	else \
# 		# PHP 5.x doesn't use "include=" by default, so we'll create our own simple config that mimics PHP 7+ for consistency
# 		mkdir php-fpm.d; \
# 		cp php-fpm.conf.default php-fpm.d/www.conf; \
# 		{ \
# 			echo '[global]'; \
# 			echo 'include=etc/php-fpm.d/*.conf'; \
# 		} | tee php-fpm.conf; \
# 	fi \
# 	&& { \
# 		echo '[global]'; \
# 		echo 'error_log = /proc/self/fd/2'; \
# 		echo; echo '; https://github.com/docker-library/php/pull/725#issuecomment-443540114'; echo 'log_limit = 8192'; \
# 		echo; \
# 		echo '[www]'; \
# 		echo '; if we send this to /proc/self/fd/1, it never appears'; \
# 		echo 'access.log = /proc/self/fd/2'; \
# 		echo; \
# 		echo 'clear_env = no'; \
# 		echo; \
# 		echo '; Ensure worker stdout and stderr are sent to the main error log.'; \
# 		echo 'catch_workers_output = yes'; \
# 		echo 'decorate_workers_output = no'; \
# 	} | tee php-fpm.d/docker.conf \
# 	&& { \
# 		echo '[global]'; \
# 		echo 'daemonize = no'; \
# 		echo; \
# 		echo '[www]'; \
# 		echo 'listen = 9000'; \
# 	} | tee php-fpm.d/zz-docker.conf

# EXPOSE 9000
# CMD ["php-fpm"]
# ##</autogenerated>##
