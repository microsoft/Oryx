#!/bin/bash

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

echo "Restoring packages..."
echo "Installing dep..."
go get -u github.com/golang/dep/cmd/dep

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