ARG BASE_IMAGE
FROM ${BASE_IMAGE}

# Keep Debian PHP packages from replacing the source-built Apache module.
RUN printf 'Package: php*\nPin: release *\nPin-Priority: -1\n' > /etc/apt/preferences.d/no-debian-php

ENV PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c"
RUN set -eux; \
    apt-get update; \
    apt-get install -y --no-install-recommends \
        $PHPIZE_DEPS \
        apache2 apache2-dev ca-certificates curl dirmngr gnupg xz-utils \
        libargon2-dev libc-client-dev libcurl4-openssl-dev libedit-dev \
        libfreetype6-dev libgmp-dev libicu-dev libjpeg62-turbo-dev \
        libkrb5-dev libldap2-dev libldb-dev libmagickwand-dev libonig-dev \
        libpng-dev libpq-dev libsodium-dev libsqlite3-dev libssl-dev \
        libtidy-dev libxml2-dev libxslt-dev libzip-dev zlib1g-dev \
        mariadb-client openssh-server openssl tcptraceroute vim wget; \
    rm -rf /var/lib/apt/lists/*; \
    a2dismod mpm_event; \
    a2enmod mpm_prefork
