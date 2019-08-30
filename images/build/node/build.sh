#!/bin/bash

set -ex

version="$1"

upgradeNpm() {
    local ver="$1"
    nodeModulesDir="/usr/local/n/versions/node/$ver/lib/node_modules"
    npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`
    if [ ! "$npm_ver" = "${npm_ver#6.}" ]; then
        echo "Upgrading node $ver's npm version from $npm_ver to 6.9.0"
        cd $nodeModulesDir
        PATH="/usr/local/n/versions/node/$ver/bin:$PATH" \
        "$nodeModulesDir/npm/bin/npm-cli.js" install npm@6.9.0
        echo
    fi
}

~/n/bin/n -d $version
cd /usr/local/n/versions/node/$version
upgradeNpm $version
mkdir -p /tmp/compressedSdk
tar -zcf /tmp/compressedSdk/nodejs-$version.tar.gz .
rm -rf /usr/local/n ~/n