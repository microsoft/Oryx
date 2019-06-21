#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

echo "Restoring packages..."
echo "Installing dep..."
go get -u github.com/golang/dep/cmd/dep
# Delete the dep sources so that we do not use it when running tests etc.
rm -rf $WORKSPACE_DIR/src/github.com/golang/dep

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
