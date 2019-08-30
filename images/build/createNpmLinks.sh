#!/bin/bash

set -ex

for ver in `ls /opt/nodejs`
do
    npm_ver=`jq -r .version /opt/nodejs/$ver/lib/node_modules/npm/package.json`
    if [ ! -d /opt/npm/$npm_ver ]; then
        mkdir -p /opt/npm/$npm_ver
        ln -s /opt/nodejs/$ver/lib/node_modules /opt/npm/$npm_ver/node_modules
        ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npm /opt/npm/$npm_ver/npm
        if [ -e /opt/nodejs/$ver/lib/node_modules/npm/bin/npx ]; then
            chmod +x /opt/nodejs/$ver/lib/node_modules/npm/bin/npx
            ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npx /opt/npm/$npm_ver/npx
        fi
    fi
done
