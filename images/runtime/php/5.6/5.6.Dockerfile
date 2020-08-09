FROM php:5.6-apache-stretch

# persistent / runtime deps
RUN set -eux; \
	apt-get update; \
	apt-get install -y --no-install-recommends \
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
        curl \
        wget \
        tcptraceroute \
        mariadb-client \
		openssl \
	; \
	rm -rf /var/lib/apt/lists/*

RUN set -eux; \
	apt-get update \
    && apt-get install -y libmcrypt-dev \
	&& docker-php-ext-install mcrypt \
	&& docker-php-ext-enable mcrypt \
    && rm -rf /var/lib/apt/lists/*