#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -eux

# Check if buster.list file exists before removing it
if [ -f /etc/apt/sources.list.d/buster.list ]; then
    rm -f /etc/apt/sources.list.d/buster.list
fi

# prevent Debian's PHP packages from being installed
# https://github.com/docker-library/php/pull/542
{
    echo 'Package: php*';
    echo 'Pin: release *';
    echo 'Pin-Priority: -1';
} > /etc/apt/preferences.d/no-debian-php

# Create the sources.list file for bookworm since it doesn't exist in the buildpack-deps image
if [ "$DEBIAN_FLAVOR" = bookworm ]
then
    {
        echo 'deb http://deb.debian.org/debian bookworm main';
        echo 'deb http://deb.debian.org/debian-security bookworm-security main';
        echo 'deb http://deb.debian.org/debian bookworm-updates main';
    } > /etc/apt/sources.list
fi

# dependencies required for running "phpize"
# (see persistent deps below)
PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c"

# persistent / runtime deps
# libcurl3 and libcurl4 both needs to be supported in ubuntu focal for php
# https://github.com/xapienz/curl-debian-scripts
if [ "$DEBIAN_FLAVOR" = focal ]
then
    add-apt-repository ppa:xapienz/curl34 -y
fi

apt-get update \
&& apt-get upgrade -y \
&& apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libonig-dev \
    --no-install-recommends && rm -r /var/lib/apt/lists/*

##<argon2>##
sed -e 's/# deb http:\/\/deb.debian.org\/debian stretch-updates/deb http:\/\/deb.debian.org\/debian stretch-updates/g' \
    -e 's/deb http:\/\/archive.debian.org\/debian stretch/deb http:\/\/deb.debian.org\/debian stretch/g' \
    -e 's/deb http:\/\/archive.debian.org\/debian-security stretch/deb http:\/\/security.debian.org\/debian-security stretch/g' \
    -e 's/stretch/buster/g' /etc/apt/sources.list > /etc/apt/sources.list.d/buster.list;
{ \
	echo 'Package: *';
	echo 'Pin: release n=buster';
	echo 'Pin-Priority: -10';
	echo;
	echo 'Package: libargon2*';
	echo 'Pin: release n=buster';
	echo 'Pin-Priority: 990';
} > /etc/apt/preferences.d/argon2-buster;
apt-get update;
apt-get install -y --no-install-recommends libsodium-dev;
##</argon2>##
