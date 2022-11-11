#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

echo "Testing packages..."

# run 'go test -v' in every directory that a go.mod file is found
goModFileName="go.mod"
for pkgDir in $WORKSPACE_DIR/src/* ; do
    if [ -d $pkgDir ]; then
        if [ -f "$pkgDir/$goModFileName" ]; then
            echo "Running 'go test' under '$pkgDir'..."
            cd $pkgDir
            go test -v
        fi
    fi
done