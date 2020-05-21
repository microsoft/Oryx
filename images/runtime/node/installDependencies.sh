#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# All users need access to node_modules at the root, as this is the location
# for packages valid for all apps.
mkdir -p /node_modules
chmod 777 /node_modules

# Npm version 6.4 has issues installing native modules like grpc,
# so upgrading it to a version which we know works fine.
npm_ver=$(npm --version)
IFS='.' read -ra SPLIT_VERSION <<< "$npm_ver"
major="${SPLIT_VERSION[0]}"
minor="${SPLIT_VERSION[1]}"
if [ "$major" == "6" ] && [ "$minor" == "4" ]; then
    echo
    echo "Upgrading npm version from $npm_ver to 6.9.0"
    npm install -g npm@6.9.0
fi

# Do NOT install PM2 from Node 14 onwards
# v10.14.0
currentNodeVersion=$(node --version)
echo "Current Node version is $currentNodeVersion"
currentNodeVersion=${currentNodeVersion#?}
IFS='.' read -ra SPLIT_VERSION <<< "$currentNodeVersion"
major="${SPLIT_VERSION[0]}"

if [ "$major" -lt "14" ]; then
    # PM2 is supported as an option when running the app,
    # so we need to make sure it is available in our images.
    npm install -g pm2@3.5.1
fi

# Application-Insights is supported as an option for telemetry when running the app,
# so we need to make sure it is available in our images.
npm install -g applicationinsights@1.7.3
