#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

source "$DIR/__dotNetCoreSdkVersions.sh"
source "$DIR/__dotNetCoreRunTimeVersions.sh"

splitSdksDir="/opt/dotnet/sdks"

allSdksDir="/opt/dotnet/all"
mkdir -p "$allSdksDir"

# Copy latest muxer and license files
cp -f "$splitSdksDir/3/dotnet" "$allSdksDir"
cp -f "$splitSdksDir/3/LICENSE.txt" "$allSdksDir"
cp -f "$splitSdksDir/3/ThirdPartyNotices.txt" "$allSdksDir"

function createLinks() {
    local sdkVersion="$1"
    local runtimeVersion="$2"
    
    cd "$splitSdksDir/$sdkVersion"

    # Find folders with name as sdk or runtime version
    find . -name "$sdkVersion" -o -name "$runtimeVersion" | while read subPath; do
        # Trim beginning 2 characters from the line which currently looks like, for example, './sdk/2.2.402'
        subPath="${subPath:2}"
        
        linkFrom="$allSdksDir/$subPath"
        linkFromParentDir=$(dirname $linkFrom)
        mkdir -p "$linkFromParentDir"

        linkTo="$splitSdksDir/$sdkVersion/$subPath"
        ln -s $linkTo $linkFrom
        echo "Created link: $linkFrom ==> $linkTo"
    done
}

createLinks "$DOT_NET_CORE_30_SDK_VERSION" "$NET_CORE_APP_30"
echo
createLinks "$DOT_NET_CORE_22_SDK_VERSION" "$NET_CORE_APP_22"
echo
createLinks "$DOT_NET_CORE_21_SDK_VERSION" "$NET_CORE_APP_21"
echo
createLinks "$DOT_NET_CORE_11_SDK_VERSION" "$NET_CORE_APP_11"