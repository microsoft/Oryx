#!/bin/bash

set -ex

version="$1"

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
mkdir -p /tmp/compressedSdk
tar -zcf /tmp/compressedSdk/nodejs-$version.tar.gz .
rm -rf /usr/local/n ~/n