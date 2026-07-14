#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# libpq-dev is for PostgreSQL
apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        libexpat1 \
        curl \
        xz-utils \
        zstd \
        gnupg \
        libpq-dev \
        default-libmysqlclient-dev \
        unzip \
        libodbc2

# TODO: Install Microsoft SQL Server ODBC Driver 
#       once it becomes available for Ubuntu 26.04

rm -rf /var/lib/apt/lists/*