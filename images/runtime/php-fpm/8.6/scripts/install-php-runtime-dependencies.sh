#!/usr/bin/env bash
# Installs every operating-system package PHP 8.6 needs when built directly on the generic Oryx
# Resolute runtime base. This replaces the shared phpFpmRuntimeBase layer while preserving its
# PHP build toolchain, native extension headers, SSH/debugging utilities, and database clients.
# Apache is omitted because this image uses Nginx/PHP-FPM. Libc-client supports the optional
# IMAP extension, and Microsoft's Resolute repository supplies the SQL Server ODBC driver.

set -euxo pipefail

export DEBIAN_FRONTEND=noninteractive

apt-get update
apt-get upgrade -y
apt-get install -y --no-install-recommends \
    $PHPIZE_DEPS \
    ca-certificates \
    curl \
    dirmngr \
    gnupg \
    libargon2-dev \
    libc-client2007e-dev \
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
    patch \
    tcptraceroute \
    odbcinst \
    unixodbc \
    unixodbc-dev \
    vim \
    wget \
    xz-utils \
    zlib1g-dev

curl -fsSL --retry 5 https://packages.microsoft.com/keys/microsoft-2025.asc \
    | gpg --batch --yes --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
gpg --batch --show-keys --with-colons /usr/share/keyrings/microsoft-prod.gpg \
    | grep -Fx 'fpr:::::::::AA86F75E427A19DD33346403EE4D7792F748182B:'
curl -fsSL --retry 5 https://packages.microsoft.com/config/ubuntu/26.04/prod.list \
    -o /etc/apt/sources.list.d/mssql-release.list
apt-get update
ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18=18.7.1.1-1

rm -rf /var/lib/apt/lists/*
