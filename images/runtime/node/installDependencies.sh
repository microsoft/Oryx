#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

source /tmp/oryx/build/__nodeVersions.sh

# All users need access to node_modules at the root, as this is the location
# for packages valid for all apps.
mkdir -p /node_modules
chmod 777 /node_modules

# Since older versions of npm cli have security vulnerabilities, we try upgrading it
# to the latest available version. However latest versions of npm do not work with very
# old versions of Node (for example: 4 or 6), so we special case here to limit the upgrade
# to only versions 8 or above
currentNodeVersion=$(node --version)
# v10.4.5 => 10.4.5
currentNodeVersion=${currentNodeVersion#?}
echo "Current Node version is $currentNodeVersion"
IFS='.' read -ra NODE_VERSION_PARTS <<< "$currentNodeVersion"
nodeVersionMajor="${NODE_VERSION_PARTS[0]}"

currentNpmVersion=$(npm --version)
echo "Version of npm: $currentNpmVersion"

# Upgrade npm to the latest available version
if [[ $nodeVersionMajor -ge 18  ]]; then
    echo "Upgrading npm..."
    npm install npm@10.5.0 -g 
    echo "Done upgrading npm."
    currentNpmVersion=$(npm --version)
    echo "Version of npm after upgrade: $currentNpmVersion"
fi

currentNodeVersion=$(node --version)
echo "Current Node version is $currentNodeVersion"
currentNodeVersion=${currentNodeVersion#?}
IFS='.' read -ra SPLIT_VERSION <<< "$currentNodeVersion"
major="${SPLIT_VERSION[0]}"

if [ "$major" -lt "10" ]; then
    echo "Installing PM2..."
    # PM2 is supported as an option when running the app,
    # so we need to make sure it is available in our images.
    npm install -g pm2@3.5.1 -loglevel silent
else
    npm install -g pm2@$PM2_VERSION -loglevel silent
fi

# Application-Insights is supported as an option for telemetry when running the app,
# so we need to make sure it is available in our images.
# Updated to 1.8.3 that doesn't emit json payload in stdout which is causing issues to customers in ant-88
npm install -g applicationinsights@$NODE_APP_INSIGHTS_SDK_VERSION -loglevel silent
