#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source $CURRENT_DIR/../__common.sh

echo
echo "Installing .NET Core SDK $DOTNET_SDK_VER ..."
echo

debianFlavor="$DEBIAN_FLAVOR"

fileName="dotnet.tar.gz"

if [ -z "$debianFlavor" ]; then
    # Use default sdk file name
    fileName="$PLATFORM_NAME-$VERSION.tar.gz"
elif [ "$debianFlavor" == "stretch" ]; then
    # Use default sdk file name
    fileName="dotnet-$DOTNET_SDK_VER.tar.gz"
else
    fileName="dotnet-$debianFlavor-$DOTNET_SDK_VER.tar.gz"
fi

downloadFileAndVerifyChecksum dotnet $DOTNET_SDK_VER $fileName

globalJsonContent="{\"sdk\":{\"version\":\"$DOTNET_SDK_VER\"}}"

# If the version is a preview version, then trim out the preview part
# Example: 3.0.100-preview4-011223 will be changed to 3.0.100
DOTNET_SDK_VER=${DOTNET_SDK_VER%%-*}

SDK_DIR=/opt/dotnet
DOTNET_DIR=$SDK_DIR/$DOTNET_SDK_VER
mkdir -p $DOTNET_DIR
tar -xzf $fileName -C $DOTNET_DIR
rm $fileName

declare -x dotnet=$SDK_DIR/$DOTNET_SDK_VER/dotnet
export dotnet

# Install MVC template based packages
if [ "$INSTALL_PACKAGES" == "true" ]
then
    echo
    echo Installing MVC template based packages ...
    mkdir warmup
    cd warmup
    echo "$globalJsonContent" > global.json
    $dotnet new mvc
    $dotnet restore
    cd ..
    rm -rf warmup
fi

if [ "$INSTALL_TOOLS" == "true" ]; then
    toolsDir="$SDK_DIR/$DOTNET_SDK_VER/tools"
    mkdir -p "$toolsDir"
    dotnet tool install --tool-path "$toolsDir" dotnet-sos
    chmod +x "$toolsDir/dotnet-sos"
    dotnet tool install --tool-path "$toolsDir" dotnet-trace
    chmod +x "$toolsDir/dotnet-trace"
    dotnet tool install --tool-path "$toolsDir" dotnet-dump
    chmod +x "$toolsDir/dotnet-dump"
    dotnet tool install --tool-path "$toolsDir" dotnet-counters
    chmod +x "$toolsDir/dotnet-counters"
    dotnet tool install --tool-path "$toolsDir" dotnet-monitor --version 6.1.*
    chmod +x "$toolsDir/dotnet-monitor"
fi
