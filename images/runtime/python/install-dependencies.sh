#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

# libpq-dev is for PostgreSQL
apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        gnupg \
        libpq-dev \
        default-libmysqlclient-dev \
        unzip \
    && rm -rf /var/lib/apt/lists/*
 
# Microsoft SQL Server 2017
export ACCEPT_EULA=Y
apt-get update \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/9/prod.list \
        > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get install -y --no-install-recommends \
        locales \
        apt-transport-https \
    && echo "en_US.UTF-8 UTF-8" > /etc/locale.gen \
    && locale-gen \
    && apt-get update \
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