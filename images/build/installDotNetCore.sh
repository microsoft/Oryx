#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

echo
echo "Installing .NET Core SDK $DOTNET_SDK_VER ..."
echo

# .NET Core 1.1 follows a different pattern for url, so we give a chance for the caller
# to specify a different url
DEFAULT_DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-sdk-$DOTNET_SDK_VER-linux-x64.tar.gz
DOTNET_SDK_URL="${DOTNET_SDK_URL:-$DEFAULT_DOTNET_SDK_URL}"

curl -SL $DOTNET_SDK_URL --output dotnet.tar.gz
if [ "$DOTNET_SDK_SHA" != "" ]
then
    echo
    echo "Verifying archive hash..."
    echo "$DOTNET_SDK_SHA dotnet.tar.gz" | sha512sum -c -
fi

globalJsonContent="{\"sdk\":{\"version\":\"$DOTNET_SDK_VER\"}}"

# If the version is a preview version, then trim out the preview part
# Example: 3.0.100-preview4-011223 will be changed to 3.0.100
DOTNET_SDK_VER=${DOTNET_SDK_VER%%-*}

SDK_DIR=/opt/dotnet/sdks
DOTNET_DIR=$SDK_DIR/$DOTNET_SDK_VER
mkdir -p $DOTNET_DIR
tar -xzf dotnet.tar.gz -C $DOTNET_DIR
rm dotnet.tar.gz

# Create a link : major.minor => major.minor.path
IFS='.' read -ra SDK_VERSION_PARTS <<< "$DOTNET_SDK_VER"
MAJOR_MINOR="${SDK_VERSION_PARTS[0]}.${SDK_VERSION_PARTS[1]}"
echo
echo "Created link from $MAJOR_MINOR to $DOTNET_SDK_VER"
ln -s $DOTNET_SDK_VER $SDK_DIR/$MAJOR_MINOR

dotnet=$SDK_DIR/$DOTNET_SDK_VER/dotnet

# Install MVC template based packages
if [ "$INSTALL_PACKAGES" != "false" ]
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
    dotnet tool install --tool-path "$toolsDir" dotnet-trace
    dotnet tool install --tool-path "$toolsDir" dotnet-dump
    dotnet tool install --tool-path "$toolsDir" dotnet-counters
fi