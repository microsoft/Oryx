#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

echo "Restoring packages..."

# Check if 'dep' is installed
EXIT_CODE=0
which dep > /dev/null 2>&1 || EXIT_CODE=$?
if [ $EXIT_CODE != 0 ]; then
    echo "Installing dep..."
    curl https://raw.githubusercontent.com/golang/dep/master/install.sh | sh
    # Delete the dep sources so that we do not use it when running tests etc.
    rm -rf $WORKSPACE_DIR/src/github.com/golang/dep
fi

tomlFileName="Gopkg.toml"
for pkgDir in $WORKSPACE_DIR/src/* ; do
    if [ -d $pkgDir ]; then
        if [ -f "$pkgDir/$tomlFileName" ]; then
            echo "Running 'dep ensure' under '$pkgDir'..."
            cd $pkgDir
            dep ensure
        else
            echo "Cound not find '$tomlFileName' under '$pkgDir'. Not running 'dep ensure'"
        fi
    fi
done