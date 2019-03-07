#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

echo
echo "Installing .NET Core SDK $DOTNET_SDK_VER ..."
echo

# .NET Core 1.1 follows a different pattern for url, so we give a chance for the caller
# to specify a different url
DEFAULT_DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-sdk-$DOTNET_SDK_VER-linux-x64.tar.gz
DOTNET_SDK_URL="${DOTNET_SDK_URL:-$DEFAULT_DOTNET_SDK_URL}"

DOTNET_DIR=/opt/dotnet/$DOTNET_SDK_VER
mkdir -p $DOTNET_DIR
curl -SL $DOTNET_SDK_URL --output dotnet.tar.gz
if [ "$DOTNET_SDK_SHA" != "" ]
then
    echo
    echo "Verifying archive hash..."
    echo "$DOTNET_SDK_SHA dotnet.tar.gz" | sha512sum -c -
fi
tar -xzf dotnet.tar.gz -C $DOTNET_DIR
rm dotnet.tar.gz

# Create a link : major.minor => major.minor.path
IFS='.' read -ra SDK_VERSION_PARTS <<< "$DOTNET_SDK_VER"
MAJOR_MINOR="${SDK_VERSION_PARTS[0]}.${SDK_VERSION_PARTS[1]}"
echo
echo "Created link from $MAJOR_MINOR to $DOTNET_SDK_VER"
ln -s $DOTNET_SDK_VER /opt/dotnet/$MAJOR_MINOR

# Install MVC template based packages
if [ "$INSTALL_PACKAGES" != "false" ]
then
    echo
    echo Installing MVC template based packages ...
    dotnet=/opt/dotnet/$DOTNET_SDK_VER/dotnet
    mkdir warmup
    cd warmup
    echo "{\"sdk\":{\"version\":\"$DOTNET_SDK_VER\"}}" > global.json
    $dotnet new mvc
    $dotnet restore
    cd ..
    rm -rf warmup
fi