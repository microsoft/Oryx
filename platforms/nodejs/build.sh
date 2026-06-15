#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

version="$1"
osFlavor="$OS_FLAVOR"

tarFileName=nodejs-$osFlavor-$version.tar.gz
nodeFileName="node-v${version}-linux-x64.tar.xz"

NODE_DOWNLOAD_URL="https://nodejs.org/dist/v${version}/${nodeFileName}"
NODE_SHASUM_URL="https://nodejs.org/dist/v${version}/SHASUMS256.txt"

INSTALL_DIR="/opt/nodejs/${version}"
mkdir -p "$INSTALL_DIR"

echo "Downloading Node.js v${version} from official source..."
curl -fsSLO "$NODE_DOWNLOAD_URL"
curl -fsSL "$NODE_SHASUM_URL" -o SHASUMS256.txt

# Verify SHA256 integrity
grep "$nodeFileName" SHASUMS256.txt | sha256sum -c -

# Extract to install directory
tar -xJf "$nodeFileName" -C "$INSTALL_DIR" --strip-components=1
rm -f "$nodeFileName" SHASUMS256.txt

# Certain versions (ex: 6.4.1) of NPM have issues installing native modules
# like 'grpc', so upgrading them to a version which we know works.
upgradeNpm() {
    local nodeDir="$INSTALL_DIR"
    local nodeModulesDir="$nodeDir/lib/node_modules"
    local npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`
    IFS='.' read -ra versionParts <<< "$npm_ver"
    local majorPart="${versionParts[0]}"
    local minorPart="${versionParts[1]}"

    if [ "$majorPart" -eq "6" ] && [ "$minorPart" -lt "9" ] ; then
        echo "Upgrading node $version's npm version from $npm_ver to 6.9.0"
        cd $nodeModulesDir
        PATH="$nodeDir/bin:$PATH" \
        "$nodeModulesDir/npm/bin/npm-cli.js" install npm@6.9.0
        echo
    fi
}

upgradeNpm

cd "$INSTALL_DIR"
mkdir -p /tmp/compressedSdk/nodejs
tar -zcf /tmp/compressedSdk/nodejs/$tarFileName .
