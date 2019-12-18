#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

appDir="$1"

function isToolInstalled() {
    local name="$1"
    local version="$2"

    result="true"
    case $name in
        node)
            if [ ! -d "/opt/nodejs/$version" ]; then
                result="false"
            fi
        ;;
        python)
            if [ ! -d "/opt/python/$version" ]; then
                result="false"
            fi
        ;;
        dotnet)
            if [ ! -d "/opt/dotnet/runtimes/$version" ]; then
                result="false"
            fi
        ;;
        php)
            if [ ! -d "/opt/php/$version" ]; then
                result="false"
            fi
        ;;
        *)
        Message="Unsupported tool name."
        ;;
    esac

    echo $result
}

function installTool() {
    local name="$1"
    local version="$2"
    
    echo "Checking if the version $2 is already installed..."
    isInstalled=`isToolInstalled $name $version`

    if [ "$isInstalled" != "true" ]; then
        echo "Installing version $version of $name..."
    fi
}

function setupTool() {
    local name="$1"
    local version="$2"

    if [ "$DISABLE_TOOL_INSTALL" != "true" ]; then
        # 1. Get list of available versions from a source
        # 2. Resolve
        installTool $name $version
    fi


}

# Detect the tools detected
detectedTools=`oryx detect "$appDir"`

# Setup the environment based on the detected tools
# For each detected tool, either use the installed version or get the latest version from the internet
# In some cases installation of those tools might take time.
IFS=',' read -ra tools <<< "$detectedTools"
for tool in "${tools[@]}"; do
    echo "$tool"
    IFS="=" read -r name value <<< "$tool"
    echo "name: $name"
    echo "version: $value"
done