#!/bin/bash

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
tomlFileName="Gopkg.toml"

echo "Restoring packages..."
echo "Installing dep..."
go get -u github.com/golang/dep/cmd/dep

function runDepEnsure() {
    pkgDir="$1"
    if [ -f "$pkgDir/$tomlFileName" ]; then
            echo "Running 'dep ensure' under '$pkgDir'..."
            cd $pkgDir
            dep ensure
        else
            echo "Cound not find '$tomlFileName' under '$pkgDir'. Not running 'dep ensure'"
    fi
}

runDepEnsure $WORKSPACE_DIR/src/common
runDepEnsure $WORKSPACE_DIR/src/dotnetcore
runDepEnsure $WORKSPACE_DIR/src/node
runDepEnsure $WORKSPACE_DIR/src/php
runDepEnsure $WORKSPACE_DIR/src/python
