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

# Microsoft SQL Server 2017
# https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server
export ACCEPT_EULA=Y \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/24.04/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18=18.6.1.1-1

mkdir -p /etc/unixODBC
cat >/etc/unixODBC/odbcinst.ini <<EOL
[ODBC Driver 18 for SQL Server]
Description=Microsoft ODBC Driver 18 for SQL Server
Driver=/opt/microsoft/msodbcsql18/lib64/libmsodbcsql-18.1.so.1.1
Threading=1
UsageCount=1

EOL

# Clean up for apt. Keeping at the very end to make sure it runs after every apt-get install.
rm -rf /var/lib/apt/lists/*