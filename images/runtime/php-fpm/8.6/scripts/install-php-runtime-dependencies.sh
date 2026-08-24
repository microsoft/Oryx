#!/usr/bin/env bash
# Installs every operating-system package PHP 8.6 needs when built directly on the generic Oryx
# Resolute runtime base. This replaces the shared phpFpmRuntimeBase layer while preserving its
# PHP build toolchain, native extension headers, SSH/debugging utilities, and database clients.
# Apache and libc-client are intentionally omitted because this image uses Nginx/PHP-FPM and the
# current PECL IMAP release does not support PHP 8.6. UnixODBC supplies the driver manager and
# command-line tools; database-specific ODBC drivers are installed separately when available.

set -euxo pipefail

export DEBIAN_FRONTEND=noninteractive

# TODO: Install msodbcsql18 when Microsoft publishes a package for Ubuntu Resolute.
apt-get update
apt-get upgrade -y
apt-get install -y --no-install-recommends \
    $PHPIZE_DEPS \
    ca-certificates \
    curl \
    dirmngr \
    gnupg \
    libargon2-dev \
    libcurl4-openssl-dev \
    libedit-dev \
    libfreetype6-dev \
    libgmp-dev \
    libicu-dev \
    libjpeg-dev \
    libjpeg-turbo8-dev \
    libkrb5-dev \
    libldap2-dev \
    libldb-dev \
    libmagickwand-dev \
    libonig-dev \
    libpng-dev \
    libpq-dev \
    libreadline-dev \
    libsodium-dev \
    libsqlite3-dev \
    libssl-dev \
    libtidy-dev \
    libxml2-dev \
    libxslt1-dev \
    libzip-dev \
    mariadb-client \
    openssh-server \
    openssl \
    tcptraceroute \
    odbcinst \
    unixodbc \
    unixodbc-dev \
    vim \
    wget \
    xz-utils \
    zlib1g-dev

rm -rf /var/lib/apt/lists/*
