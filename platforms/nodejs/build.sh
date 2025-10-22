#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

version="$1"
osFlavor="$OS_FLAVOR"

tarFileName=nodejs-$osFlavor-$version.tar.gz

# Certain versions (ex: 6.4.1) of NPM have issues installing native modules
# like 'grpc', so upgrading them to a version whch we know works.
upgradeNpm() {
    local node_ver="$1"

    local nodeDir="/usr/local/n/versions/node/$node_ver"
    local nodeModulesDir="$nodeDir/lib/node_modules"
    local npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`
    IFS='.' read -ra versionParts <<< "$npm_ver"
    local majorPart="${versionParts[0]}"
    local minorPart="${versionParts[1]}"

    if [ "$majorPart" -eq "6" ] && [ "$minorPart" -lt "9" ] ; then
        echo "Upgrading node $node_ver's npm version from $npm_ver to 6.9.0"
        cd $nodeModulesDir
        PATH="$nodeDir/bin:$PATH" \
        "$nodeModulesDir/npm/bin/npm-cli.js" install npm@6.9.0
        echo
    fi
}

~/n/bin/n -d $version
upgradeNpm $version
cd /usr/local/n/versions/node/$version
mkdir -p /tmp/compressedSdk/nodejs
tar -zcf /tmp/compressedSdk/nodejs/$tarFileName .
