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
apt-get update && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
    --no-install-recommends && rm -r /var/lib/apt/lists/*

##<argon2>##
sed -e 's/stretch/buster/g' /etc/apt/sources.list > /etc/apt/sources.list.d/buster.list;
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
apt-get install -y --no-install-recommends libargon2-dev libsodium-dev;
##</argon2>##
