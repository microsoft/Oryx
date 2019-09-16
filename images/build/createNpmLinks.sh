#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

# This script creates folders of the form '/opt/npm/<npm-version>' with the 'npm' file pointing/linking
# to version that is installed as part of a node version.
# This allows end users to use a version of npm which is not baked into a specific version of node.
for ver in `ls /opt/nodejs`
do
    nodeModulesDir="/opt/nodejs/$ver/lib/node_modules"
    npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`
    if [ ! -d /opt/npm/$npm_ver ]; then
        mkdir -p /opt/npm/$npm_ver
        ln -s $nodeModulesDir /opt/npm/$npm_ver/node_modules
        ln -s $nodeModulesDir/npm/bin/npm /opt/npm/$npm_ver/npm
        if [ -e $nodeModulesDir/npm/bin/npx ]; then
            chmod +x $nodeModulesDir/npm/bin/npx
            ln -s $nodeModulesDir/npm/bin/npx /opt/npm/$npm_ver/npx
        fi
    fi
done
