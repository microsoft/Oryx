#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

echo
echo "Installing .NET Core SDK $DOTNET_SDK_VER ..."
echo

ORYX_BLOB_URL_BASE="https://oryxsdksdev.blob.core.windows.net/dotnet"
DOTNET_SDK_URL=$ORYX_BLOB_URL_BASE/dotnet-$DOTNET_SDK_VER.tar.gz

curl -SL $DOTNET_SDK_URL --output dotnet.tar.gz

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

# Install MVC template based packages
if [ "$INSTALL_PACKAGES" != "false" ]
then
    echo
    echo Installing MVC template based packages ...
    dotnet=$SDK_DIR/$DOTNET_SDK_VER/dotnet
    mkdir warmup
    cd warmup
    echo "$globalJsonContent" > global.json
    $dotnet new mvc
    $dotnet restore
    cd ..
    rm -rf warmup
fi