#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# Dependencies for various ruby and rubygem installations
apt-get update -qq \
    && apt-get install -y --no-install-recommends \
        libreadline-dev \
        bzip2 \
        build-essential \
        libssl-dev \
        zlib1g-dev \
        libpq-dev \
        libsqlite3-dev \
        patch \
        gawk \
        g++ \
        gcc \
        make \
        libc6-dev \
        patch \
        libreadline6-dev \
        libyaml-dev \
        sqlite3 \
        autoconf \
        libgdbm-dev \
        libncurses5-dev \
        automake \
        libtool \
        bison \
        pkg-config \
        libffi-dev \
        bison \
        libxslt-dev \
        libxml2-dev \
        wget \
        git \
        net-tools \
        dnsutils \
        curl \
        tcpdump \
        iproute2 \
        # SQL Server gem support
        unixodbc-dev \
        vim \
        tcptraceroute \
    && rm -rf /var/lib/apt/lists/*

# install libssl1.0.2, and libssl1.1
# these links need to be updated constantly
# maybe save a copy locally
wget http://ftp.us.debian.org/debian/pool/main/o/openssl1.0/libssl1.0.2_1.0.2u-1~deb9u1_amd64.deb \
    && dpkg -i libssl1.0.2_1.0.2u-1~deb9u1_amd64.deb \
    && wget http://ftp.us.debian.org/debian/pool/main/libx/libxcrypt/libcrypt1-udeb_4.4.16-1_amd64.udeb \
    && dpkg -i libcrypt1-udeb_4.4.16-1_amd64.udeb \
    && wget http://http.us.debian.org/debian/pool/main/g/glibc/libc6-udeb_2.28-10_amd64.udeb \
    && dpkg -i --force-overwrite libc6-udeb_2.28-10_amd64.udeb \
    && wget http://ftp.us.debian.org/debian/pool/main/o/openssl/libcrypto1.1-udeb_1.1.0l-1~deb9u1_amd64.udeb \
    && dpkg -i --force-overwrite libcrypto1.1-udeb_1.1.0l-1~deb9u1_amd64.udeb \
    && wget http://ftp.us.debian.org/debian/pool/main/o/openssl/libssl1.1-udeb_1.1.0l-1~deb9u1_amd64.udeb \
    && dpkg -i --force-overwrite libssl1.1-udeb_1.1.0l-1~deb9u1_amd64.udeb

# Clean up for apt. Keeping at the very end to make sure it runs after every apt-get install.
rm -rf /var/lib/apt/lists/*