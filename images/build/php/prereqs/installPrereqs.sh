#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -eux

# prevent Debian's PHP packages from being installed
# https://github.com/docker-library/php/pull/542
{
    echo 'Package: php*';
    echo 'Pin: release *';
    echo 'Pin-Priority: -1';
} > /etc/apt/preferences.d/no-debian-php

# dependencies required for running "phpize"
# (see persistent deps below)
PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c"

# persistent / runtime deps
# libcurl3 and libcurl4 both needs to be supported in ubuntu focal for php
# https://github.com/xapienz/curl-debian-scripts
add-apt-repository ppa:xapienz/curl34 -y \
apt-get update \
&& apt-get upgrade -y \
&& apt-get install -y \
        sed \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
    --no-install-recommends && rm -rf /var/lib/apt/lists/*

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
apt-get update
apt-get install -y --no-install-recommends \
    libsodium-dev
rm -rf /var/lib/apt/lists/*
##</argon2>##
