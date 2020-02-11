FROM oryx-base-buster

# prevent Debian's PHP packages from being installed
# https://github.com/docker-library/php/pull/542
RUN set -eux; \
	{ \
		echo 'Package: php*'; \
		echo 'Pin: release *'; \
		echo 'Pin-Priority: -1'; \
	} > /etc/apt/preferences.d/no-debian-php

# dependencies required for running "phpize"
# (see persistent deps below)
ENV PHPIZE_DEPS \
		autoconf \
		dpkg-dev \
		file \
		g++ \
		gcc \
		libc-dev \
		make \
		pkg-config \
		re2c

# persistent / runtime deps
RUN set -eux; \
	apt-get update; \
	apt-get install -y --no-install-recommends \
		$PHPIZE_DEPS \
		ca-certificates \
		curl \
		xz-utils \
# Start of some more php fpm dependencies
		libargon2-dev \
		libcurl4-openssl-dev \
		libedit-dev \
		libedit2 \
		libonig-dev \
		libsodium-dev \
		libsqlite3-dev \
		libssl-dev \
		libxml2-dev \
		zlib1g-dev \
# End of some more php fpm dependencies
		libzip-dev \
		libpng-dev \
		libjpeg-dev \
		libpq-dev \
		libldap2-dev \
		libldb-dev \
		libicu-dev \
		libgmp-dev \
		libmagickwand-dev \
		libc-client-dev \
		libtidy-dev \
		libkrb5-dev \
		libxslt-dev \
		unixodbc-dev \
		openssh-server \
		vim \
		wget \
		tcptraceroute \
		mariadb-client \
		openssl \
		nginx \
	; \
	rm -rf /var/lib/apt/lists/*

# do NOT merge this content with above line because the 
# above line is shared across all php images
# Install the Microsoft SQL Server PDO driver on supported versions only.
#  - https://docs.microsoft.com/en-us/sql/connect/php/installation-tutorial-linux-mac
#  - https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server
RUN set -eux \
	&& apt-get update \
	&& apt-get install -y --no-install-recommends \
		gnupg2 \
		apt-transport-https \
	&& curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
	&& curl https://packages.microsoft.com/config/debian/9/prod.list > /etc/apt/sources.list.d/mssql-release.list \
	&& apt-get update \
	&& ACCEPT_EULA=Y apt-get install -y msodbcsql17 unixodbc-dev
	
