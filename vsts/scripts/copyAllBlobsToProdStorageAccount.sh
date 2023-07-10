#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

azCopyDir="/tmp/azcopy-tool"

function blobContainerExistsInProd() {
	local containerName="$1"
	local exitCode=1
	curl -I "$PROD_SDK_STORAGE_BASE_URL/$containerName?restype=container" 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
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

function copyBlobContainerToProd() {
    set +x
    local platformName="$1"

    if shouldOverwriteSdk || shouldOverwritePlatformSdk $platformName; then
        echo
        echo "Overwriting blob container '$platformName' in storage account '$destinationSdk'."
        # azcopy copy [source] [destination] [flags]
        if [ $dryRun == "False" ] ; then
            "$azCopyDir/azcopy" copy \
                "$sourceSdk/$platformName$sasToken" \ 
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" --overwrite true --recursive
        else
            "$azCopyDir/azcopy" copy \
                "$sourceSdk/$platformName$sasToken" \ 
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" --overwrite true --recursive --dry-run
        fi
    elif blobContainerExistsInProd $platformName; then
        echo
        echo "Blob container '$platformName' already exists in Prod storage account. Skipping copying it..."
    else
        echo
        echo "Blob container '$platformName' does not exist in Prod. Copying it from $sourceSdk..."
        # azcopy copy [source] [destination] [flags]
        if [ $dryRun == "False" ] ; then
            "$azCopyDir/azcopy" copy \
                "$sourceSdk/$platformName$sasToken" \ 
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" --overwrite false --recursive
        else
            "$azCopyDir/azcopy" copy \
                "$sourceSdk/$platformName$sasToken" \ 
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" --overwrite false --recursive --dry-run
        fi
    fi
    set -x
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

sourceSdk=""
sasToken=""

if [ "$1" = $SANDBOX_SDK_STORAGE_BASE_URL ]; then
    sourceSdk=$SANDBOX_SDK_STORAGE_BASE_URL
    sasToken=$SANDBOX_STORAGE_SAS_TOKEN
elif [ "$1" = $DEV_SDK_STORAGE_BASE_URL ]; then
    sourceSdk=$DEV_SDK_STORAGE_BASE_URL
    sasToken=$DEV_STORAGE_SAS_TOKEN
elif [ "$1" = $PRIVATE_STAGING_SDK_STORAGE_BASE_URL ]; then
    sourceSdk=$PRIVATE_STAGING_SDK_STORAGE_BASE_URL
    set +x
    sasToken=$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN
    set -x
else
	echo "Error: $1 is an invalid source storage account url."
	exit 1
fi

dryRun=$2
if [ $dryRun != "True" ] && [ $dryRun != "False" ]; then
	echo "Error: Dry run must be True or False. Was: '$dryRun'"
	exit 1
fi

copyBlobContainerToProd "dotnet"
copyBlobContainerToProd "python"
copyBlobContainerToProd "nodejs"
copyBlobContainerToProd "php"
copyBlobContainerToProd "php-composer"
copyBlobContainerToProd "ruby"
copyBlobContainerToProd "java"
copyBlobContainerToProd "maven"
copyBlobContainerToProd "golang"
