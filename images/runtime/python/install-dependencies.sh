#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# libpq-dev is for PostgreSQL
apt-get update \
    && apt-get install -y --no-install-recommends \
        libexpat1 \
        curl \
        gnupg \
        libpq-dev \
        default-libmysqlclient-dev \
        unzip \
        libodbc1 \
        apt-transport-https \
    && rm -rf /var/lib/apt/lists/*
 
# Microsoft SQL Server 2017
export ACCEPT_EULA=Y \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/9/prod.list \
        > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        locales \
        apt-transport-https \
    && echo "en_US.UTF-8 UTF-8" > /etc/locale.gen \
    && locale-gen \
    && apt-get -y --no-install-recommends install \
        unixodbc-dev \
        msodbcsql17

mkdir -p /etc/unixODBC
cat >/etc/unixODBC/odbcinst.ini <<EOL
[ODBC Driver 17 for SQL Server]
Description=Microsoft ODBC Driver 17 for SQL Server
Driver=/opt/microsoft/msodbcsql17/lib64/libmsodbcsql-17.2.so.0.1
Threading=1
UsageCount=1
EOL

# Use Gunicorn as our WSGI Servier
pip install --upgrade pip
pip install gunicorn

ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx

# Clean up for apt. Keeping at the very end to make sure it runs after every apt-get install.
rm -rf /var/lib/apt/lists/*