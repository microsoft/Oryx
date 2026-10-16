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
        ca-certificates \
        curl \
        xz-utils \
        zstd \
        gnupg \
        libpq-dev \
        default-libmysqlclient-dev \
        unzip \
        libodbc2

# Microsoft ODBC Driver 18 for SQL Server
curl -fsSL --retry 5 https://packages.microsoft.com/keys/microsoft-2025.asc \
    | gpg --batch --yes --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
# Fail the build if the key is not the expected Microsoft signing key
gpg --batch --show-keys --with-colons /usr/share/keyrings/microsoft-prod.gpg \
    | grep -Fx 'fpr:::::::::AA86F75E427A19DD33346403EE4D7792F748182B:'
curl -fsSL --retry 5 https://packages.microsoft.com/config/ubuntu/26.04/prod.list \
    -o /etc/apt/sources.list.d/mssql-release.list
apt-get update
ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18=18.7.1.1-1

rm -rf /var/lib/apt/lists/*