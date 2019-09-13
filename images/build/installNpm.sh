#!/bin/bash

set -ex

for ver in `ls /opt/node`
do
    nodeModulesDir="/opt/node/$ver/lib/node_modules"
    npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`
    if [ ! "$npm_ver" = "${npm_ver#6.}" ]; then
        echo "Upgrading node $ver's npm version from $npm_ver to 6.9.0"
        cd $nodeModulesDir
        PATH="/opt/node/$ver/bin:$PATH" \
        "$nodeModulesDir/npm/bin/npm-cli.js" install npm@6.9.0
        echo
    fi
done

for ver in `ls /opt/node`
do
    npm_ver=`jq -r .version /opt/node/$ver/lib/node_modules/npm/package.json`
    if [ ! -d /opt/npm/$npm_ver ]; then
        mkdir -p /opt/npm/$npm_ver
        ln -s /opt/node/$ver/lib/node_modules /opt/npm/$npm_ver/node_modules
        ln -s /opt/node/$ver/lib/node_modules/npm/bin/npm /opt/npm/$npm_ver/npm
        if [ -e /opt/node/$ver/lib/node_modules/npm/bin/npx ]; then
            chmod +x /opt/node/$ver/lib/node_modules/npm/bin/npx
            ln -s /opt/node/$ver/lib/node_modules/npm/bin/npx /opt/npm/$npm_ver/npx
        fi
    fi
done
