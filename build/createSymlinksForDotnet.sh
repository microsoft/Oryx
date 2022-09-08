#!/bin/bash
#--------------------------------------------------------------------------------------------------------------
#  Copyright (c) Microsoft Corporation. All rights reserved.
#  Licensed under the MIT License. See https://go.microsoft.com/fwlink/?linkid=2090316 for license information.
#--------------------------------------------------------------------------------------------------------------

set -e

oryxImageDetectorFile="/opt/oryx/.imagetype"
SYMLINK_DIRECTORY_NAME=""

if [ -f "$oryxImageDetectorFile" ] && grep -q "vso-" "$oryxImageDetectorFile"; then
    echo "image detector file exists, image is vso based.."
    SYMLINK_DIRECTORY_NAME="codespace"
fi

echo "Symlink directory name: $SYMLINK_DIRECTORY_NAME"

splitSdksDir="/opt/dotnet"

allSdksDir="/home/$SYMLINK_DIRECTORY_NAME/.dotnet"
mkdir -p "$allSdksDir"

# Copy latest muxer and license files
cp -f "$splitSdksDir/lts/dotnet" "$allSdksDir"
cp -f "$splitSdksDir/lts/LICENSE.txt" "$allSdksDir"
cp -f "$splitSdksDir/lts/ThirdPartyNotices.txt" "$allSdksDir"

function createLinks() {
    local sdkVersion="$1"

    installedDir="$splitSdksDir/$sdkVersion"

    for x in $(find $installedDir/shared/Microsoft.AspNetCore.App/ -mindepth 1 -maxdepth 1 -type d | cut -c 1-)
    do
       echo "folder: $x"
       linkDest="$allSdksDir/shared/Microsoft.AspNetCore.App/$sdkVersion"
       linkFromParent=$(dirname $linkDest)

       mkdir -p "$linkFromParent"
       linkSource="$x"
       ln -sdf $linkSource $linkDest
    done

    for y in $(find $installedDir/shared/Microsoft.NETCore.App/ -mindepth 1 -maxdepth 1 -type d | cut -c 1-)
    do
       echo "directory: $y"
       linkDest="$allSdksDir/shared/Microsoft.NETCore.App/$sdkVersion"
       linkFromParent=$(dirname $linkDest)

       mkdir -p "$linkFromParent"
       linkSource="$y"
       ln -sdf $linkSource $linkDest
    done

    for z in $(find $installedDir/host/fxr/ -mindepth 1 -maxdepth 1 -type d | cut -c 1-)
    do
       echo "folder: $z"
       linkDest="$allSdksDir/host/fxr/$sdkVersion"
       linkFromParent=$(dirname $linkDest)

       mkdir -p "$linkFromParent"
       linkSource="$z"
       ln -sdf $linkSource $linkDest
    done

    cd "$installedDir"
    # Find directories with the name being a version number like 3.1.0 or 3.1.301
    find . -maxdepth 3 -type d -regex '.*/[0-9]\.[0-9]\.[0-9]+' | while read subPath; do
        # Trim beginning 2 characters from the line which currently looks like, for example, './sdk/2.2.402'
        subPath="${subPath:2}"
        linkFrom="$allSdksDir/$subPath"

        linkFromParentDir=$(dirname $linkFrom)
        mkdir -p "$linkFromParentDir"

        linkTo="$installedDir/$subPath"

        if [ -L ${linkTo} ] ; then
            if [ -e ${linkTo} ] ; then
                echo "$linkTo already exists"
            else
                echo "$linkTo is a Broken link, creating again ..."
                ln -sTf $linkTo $linkFrom
            fi
        else
            echo "$linkTo is missing, creating ..."
            ln -sTf $linkTo $linkFrom
        fi
    done

    # Find directories with the name having a preview version number like 3.0.100-preview.3.21202.5
    find . -maxdepth 2 -type d -regex '.*/[0-9]\.[0-9]\.[0-9]+.*' | while read subPath; do
        subPath="${subPath:2}"

        linkFrom="$allSdksDir/$subPath"
        linkFromParentDir=$(dirname $linkFrom)
        mkdir -p "$linkFromParentDir"

        linkTo="$installedDir/$subPath"

        if [ -L ${linkTo} ] ; then
            if [ -e ${linkTo} ] ; then
                echo "$linkTo already exists"
            else
                echo "$linkTo is a Broken link, creating again ..."
                ln -sTf $linkTo $linkFrom
            fi
        else
            echo "$linkTo is missing, creating ..."
            ln -sTf $linkTo $linkFrom
        fi
    done
}

# Dynamically find and link all installed dotnet SDKs
find /opt/dotnet/*.*.*/sdk -maxdepth 1 -type d -name "*.*.*" | while read SDK_PATH; do
    SDK_VERSION="$(basename ${SDK_PATH})"
    createLinks "$SDK_VERSION"
done