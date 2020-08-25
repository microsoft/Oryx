#!/bin/bash

# This script is referenced from official docker library: 
# https://github.com/docker-library/ruby/blob/master/Dockerfile-debian.template
# some of ruby's build scripts are written in ruby
#   we purge system ruby later to make sure our final image uses what we just built

# skip installing gem documentation
set -eux; \
	mkdir -p /usr/local/etc; \
	{ \
		echo 'install: --no-document'; \
		echo 'update: --no-document'; \
	} >> /usr/local/etc/gemrc

LANG=C.UTF-8
RUBY_MAJOR=$RUBY_MAJOR_VERSION
RUBY_VERSION=$RUBY_VERSION
RUBY_DOWNLOAD_SHA256=$RUBY_SHA256
RUBYGEMS_VERSION=$GEM_VERSION

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
		--build="$gnuArch" \
		--disable-install-doc \
		--enable-shared \
	; \
	make -j "$(nproc)"; \
	make install; \
	\
	apt-mark auto '.*' > /dev/null; \
	apt-mark manual $savedAptMark > /dev/null; \
	find /usr/local -type f -executable -not \( -name '*tkinter*' \) -exec ldd '{}' ';' \
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

# make sure bundled "rubygems" is older than GEM_VERSION (https://github.com/docker-library/ruby/issues/246)
ruby -e 'exit(Gem::Version.create(ENV["GEM_VERSION"]) > Gem::Version.create(Gem::VERSION))'; \
gem update --system "$GEM_VERSION" && rm -r /root/.gem/; \

# verify we have no "ruby" packages installed
! dpkg -l | grep -i ruby; \
[ "$(command -v ruby)" = '/usr/local/bin/ruby' ]; \

# rough smoke test
ruby --version; \
gem --version; \
bundle --version

# don't create ".bundle" in all our apps
GEM_HOME=/usr/local/bundle
BUNDLE_SILENCE_ROOT_WARNING=1
BUNDLE_APP_CONFIG="$GEM_HOME"
PATH=$GEM_HOME/bin:$PATH

# adjust permissions of a few directories for running "gem install" as an arbitrary user
mkdir -p "$GEM_HOME" && chmod 777 "$GEM_HOME" 