#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

azCopyDir="/tmp/azcopy-tool"

function blobExistsInProd() {
	local containerName="$1"
	local blobName="$2"
	local exitCode=1
	curl -I $PROD_SDK_STORAGE_BASE_URL/$containerName/$blobName 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
	grep "HTTP/1.1 200 OK" /tmp/curlOut.txt &> /dev/null
	exitCode=$?
	rm -f /tmp/curlOut.txt
	rm -f /tmp/curlError.txt
	if [ $exitCode -eq 0 ]; then
		return 0
	else
        return 1
	fi
}

function copyBlob() {
    local platformName="$1"
    local blobName="$2"

    if shouldOverwriteSdk || shouldOverwritePlatformSdk $platformName; then
        echo
        echo "Blob '$blobName' exists in Prod storage container '$platformName'. Overwriting it..."
        "$azCopyDir/azcopy" copy \
            "$DEV_SDK_STORAGE_BASE_URL/$platformName/$blobName$DEV_STORAGE_SAS_TOKEN" \
            "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN" --overwrite true
    elif blobExistsInProd $platformName $blobName; then
        echo
        echo "Blob '$blobName' already exists in Prod storage container '$platformName'. Skipping copying it..."
    else
        echo
        echo "Blob '$blobName' does not exist in Prod storage container '$platformName'. Copying it..."
        "$azCopyDir/azcopy" copy \
            "$DEV_SDK_STORAGE_BASE_URL/$platformName/$blobName$DEV_STORAGE_SAS_TOKEN" \
            "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN"
    fi
}

function copyPlatformBlobsToProd() {
    local platformName="$1"
    local versionsFile="$REPO_DIR/platforms/$platformName/versionsToBuild.txt"

    if [ "$platformName" == "php-composer" ]; then
        versionsFile="$REPO_DIR/platforms/php/composer/versionsToBuild.txt"
    elif [ "$platformName" == "maven" ]; then
        versionsFile="$REPO_DIR/platforms/java/maven/versionsToBuild.txt"
    fi

    # Here '3' is a file descriptor which is specifically used to read the versions file.
    # This is used since 'azcopy' command seems to also be using the standard file descriptor for stdin '0'
    # which causes some issues when trying to loop through the lines of the file.
    while IFS= read -u 3 -r line || [[ -n $line ]]
	do
        # Ignore whitespace and comments
        if [ -z "$line" ] || [[ $line = \#* ]] ; then
            continue
        fi

        IFS=',' read -ra LINE_INFO <<< "$line"
        version=$(echo -e "${LINE_INFO[0]}" | sed -e 's/^[[:space:]]*//')
        copyBlob "$platformName" "$platformName-$version.tar.gz"
        copyBlob "$platformName" "$platformName-focal-scm-$version.tar.gz"
        copyBlob "$platformName" "$platformName-buster-$version.tar.gz"
	done 3< "$versionsFile"

    copyBlob "$platformName" defaultVersion.txt
}

if [ ! -f "$azCopyDir/azcopy" ]; then
    curl -SL https://aka.ms/downloadazcopy-v10-linux -o /tmp/azcopy_download.tar.gz
    tar -xvf /tmp/azcopy_download.tar.gz -C /tmp
    rm -rf /tmp/azcopy_download.tar.gz
    mkdir -p $azCopyDir
    cp /tmp/azcopy_linux_amd64_*/azcopy $azCopyDir

    echo "Version of azcopy tool being used:"
    $azCopyDir/azcopy --version
fi

copyPlatformBlobsToProd "dotnet"
copyPlatformBlobsToProd "python"
copyPlatformBlobsToProd "nodejs"
copyPlatformBlobsToProd "php"
copyPlatformBlobsToProd "php-composer"
copyPlatformBlobsToProd "ruby"
copyPlatformBlobsToProd "java"
copyPlatformBlobsToProd "maven"
copyPlatformBlobsToProd "golang"
