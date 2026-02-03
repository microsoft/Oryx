#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -eux

# Supported OS flavors: bullseye, bookworm (Debian), noble (Ubuntu 24.04)

# prevent Debian's PHP packages from being installed
# https://github.com/docker-library/php/pull/542
{
    echo 'Package: php*';
    echo 'Pin: release *';
    echo 'Pin-Priority: -1';
} > /etc/apt/preferences.d/no-debian-php

# Create the sources.list file for bookworm since it doesn't exist in the buildpack-deps image
if [ "$OS_FLAVOR" = "bookworm" ]; then
    {
        echo 'deb http://deb.debian.org/debian bookworm main';
        echo 'deb http://deb.debian.org/debian-security bookworm-security main';
        echo 'deb http://deb.debian.org/debian bookworm-updates main';
    } > /etc/apt/sources.list
fi

# dependencies required for running "phpize"
# (see persistent deps below)
PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c"

# Set DEBIAN_FRONTEND environment variable
export DEBIAN_FRONTEND=noninteractive

apt-get update \
&& apt-get upgrade -y \
&& apt-get install -y \
        apt-utils \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libonig-dev \
        libsodium-dev \
    --no-install-recommends && rm -r /var/lib/apt/lists/*
