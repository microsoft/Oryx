#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

echo "Restoring packages and building..."

# run build script to install dependencies and build modules
goModFileName="go.mod"
goSource="/go/src/"
for pkgDir in $WORKSPACE_DIR/src/* ; do
    if [ -d $pkgDir ]; then
        if [ -f "$pkgDir/$goModFileName" ]; then
            echo "Running './build.sh ${pkgDir#$goSource} ${pkgDir#$goSource}' under '$pkgDir'..."
            ./build.sh ${pkgDir#$goSource} ${pkgDir#$goSource}
        else
            echo "Cound not find '$goModFileName' under '$pkgDir'. Not running 'build.sh'"
        fi
    fi
done