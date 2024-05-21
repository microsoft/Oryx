#!/bin/bash

# This script is referenced from official docker library: 
# https://github.com/docker-library/ruby/blob/master/Dockerfile-debian.template
# some of ruby's build scripts are written in ruby
# we purge system ruby later to make sure our final image uses what we just built

set -eux

LANG=C.UTF-8
RUBY_MAJOR_VERSION=${RUBY_VERSION:0:3}
INSTALLATION_PREFIX=/opt/ruby/$RUBY_VERSION
debianFlavor=$DEBIAN_FLAVOR
rubySdkFileName=""

if [ "$debianFlavor" == "stretch" ]; then
	# Use default python sdk file name
	rubySdkFileName=ruby-$RUBY_VERSION.tar.gz
else
	rubySdkFileName=ruby-$debianFlavor-$RUBY_VERSION.tar.gz
	apt-get update; \
	apt-get install -y --no-install-recommends \
		autoconf \
		libssl-dev \
		zlib1g-dev \
		libreadline-dev \
		build-essential
fi

# skip installing gem documentation
set -eux; \
	mkdir -p $INSTALLATION_PREFIX/etc; \
	{ \
		echo 'install: --no-document'; \
		echo 'update: --no-document'; \
	} >> $INSTALLATION_PREFIX/etc/gemrc

set -eux; \
	\
	savedAptMark="$(apt-mark showmanual)"; \
	apt-get update; \
	apt-get install -y --no-install-recommends \
		bison \
		dpkg-dev \
		libgdbm-dev \
		ruby \
	; \
	rm -rf /var/lib/apt/lists/*; \
	\
	wget -O ruby.tar.xz "https://cache.ruby-lang.org/pub/ruby/$RUBY_MAJOR_VERSION/ruby-$RUBY_VERSION.tar.xz"; \
	echo "$RUBY_SHA256 *ruby.tar.xz" | sha256sum --check --strict; \
	\
	mkdir -p /usr/src/ruby; \
	tar -xJf ruby.tar.xz -C /usr/src/ruby --strip-components=1; \
	rm ruby.tar.xz; \
	\
	cd /usr/src/ruby; \
	\

# hack in "ENABLE_PATH_CHECK" disabling to suppress:
#   warning: Insecure world writable dir
	{ \
		echo '#define ENABLE_PATH_CHECK 0'; \
		echo; \
		cat file.c; \
	} > file.c.new; \
	mv file.c.new file.c; \
	\
	autoconf; \
	gnuArch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)"; \
	./configure \
        --prefix=$INSTALLATION_PREFIX \
		--build="$gnuArch" \
		--disable-install-doc \
		--enable-shared \
	; \
	make -j "$(nproc)"; \
	make install; \
	\
	apt-mark auto '.*' > /dev/null; \
	apt-mark manual $savedAptMark > /dev/null; \
	find $INSTALLATION_PREFIX -type f -executable -not \( -name '*tkinter*' \) -exec ldd '{}' ';' \
		| awk '/=>/ { print $(NF-1) }' \
		| sort -u \
		| xargs -r dpkg-query --search \
		| cut -d: -f1 \
		| sort -u \
		| xargs -r apt-mark manual \
	; \
	apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false; \
	\
	cd /; \
	rm -r /usr/src/ruby; \

rubyBinDir="$INSTALLATION_PREFIX/bin"

echo
echo "Contents of '$rubyBinDir':"
ls -l $rubyBinDir
echo

# make sure bundled "rubygems" is older than GEM_VERSION (https://github.com/docker-library/ruby/issues/246)
$rubyBinDir/ruby -e 'exit(Gem::Version.create(ENV["GEM_VERSION"]) > Gem::Version.create(Gem::VERSION))'; \
$rubyBinDir/gem update --system "$GEM_VERSION"; \

# verify we have no "ruby" packages installed
! dpkg -l | grep -i $rubyBinDir/ruby; \
[ "$(command -v $rubyBinDir/ruby)" = "$rubyBinDir/ruby" ]; \

# rough smoke test
$rubyBinDir/ruby --version; \
$rubyBinDir/gem --version; \
$rubyBinDir/bundle --version

# don't create ".bundle" in all our apps
GEM_HOME=$INSTALLATION_PREFIX/bundle
BUNDLE_SILENCE_ROOT_WARNING=1
BUNDLE_APP_CONFIG="$GEM_HOME"
PATH=$GEM_HOME/bin:$PATH
LD_LIBRARY_PATH=$INSTALLATION_PREFIX/lib

# adjust permissions of a few directories for running "gem install" as an arbitrary user
mkdir -p "$GEM_HOME"; \
chmod 777 "$GEM_HOME" 

compressedSdkDir="/tmp/compressedSdk"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$rubySdkFileName .