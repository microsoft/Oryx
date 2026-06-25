#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# Core runtime libs + DB driver dev headers + tools
apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        wget \
        less \
        git \
        gnupg \
        libexpat1 \
        libodbc2 \
        libpq-dev \
        default-libmysqlclient-dev

# Microsoft ODBC Driver 18 for SQL Server (pyodbc target).
# Uses the official packages.microsoft.com ubuntu/24.04 prod feed.
# See https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
    | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
curl -fsSL https://packages.microsoft.com/config/ubuntu/24.04/prod.list \
    > /etc/apt/sources.list.d/mssql-release.list

apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18

mkdir -p /etc/unixODBC
cat >/etc/unixODBC/odbcinst.ini <<'EOL'
[ODBC Driver 18 for SQL Server]
Description=Microsoft ODBC Driver 18 for SQL Server
Driver=/opt/microsoft/msodbcsql18/lib64/libmsodbcsql-18.1.so.1.1
Threading=1
UsageCount=1
EOL

# Remove gnupg — only needed to dearmor the key above.
apt-get purge -y --auto-remove gnupg

# Clean up — keep at the very end so every apt-get install above is included.
rm -rf /var/lib/apt/lists/*
