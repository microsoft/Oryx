#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

version="$1"
debianFlavor="$DEBIAN_FLAVOR"

tarFileName="nodejs-$version.tar.gz"
    
if [ "$debianFlavor" == "stretch" ]; then
   # Use default sdk file name
	tarFileName=nodejs-$version.tar.gz
else
    tarFileName=nodejs-$debianFlavor-$version.tar.gz
fi

upgradeNpm() {
    local node_ver="$1"
    local nodeDir="/usr/local/n/versions/node/$node_ver"

    echo "Upgrading node $node_ver's npm version to 7"
    PATH="$nodeDir/bin:$PATH" \
    $nodeDir/bin/npm install -g npm@7
    echo
}

~/n/bin/n -d $version
upgradeNpm $version
cd /usr/local/n/versions/node/$version
mkdir -p /tmp/compressedSdk
tar -zcf /tmp/compressedSdk/$tarFileName .
rm -rf /usr/local/n ~/n
