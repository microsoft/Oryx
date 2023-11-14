#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

azCopyDir="/tmp/azcopy-tool"

function blobContainerExistsInDestination() {
	local containerName="$1"
	local exitCode=1
	curl -I "$destinationSdkUrl/$containerName?restype=container" 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
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

function copyBlobContainerFromProdToDestination() {
    set +x
    local platformName="$1"

    if [ $overwrite == "True" ] ; then
        echo
        echo "Overwriting blob container '$platformName' in storage account '$destinationSdkUrl'."
        # azcopy copy [source] [destination] [flags]
        if [ $dryRun == "False" ] ; then
            "$azCopyDir/azcopy" copy \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" \
                "$destinationSdkUrl/$platformName$sasToken" --overwrite true --recursive
        else
            "$azCopyDir/azcopy" copy \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" \
                "$destinationSdkUrl/$platformName$sasToken" --overwrite true --recursive --dry-run
        fi
    elif blobContainerExistsInDestination $platformName; then
        echo
        echo "Blob container '$platformName' already exists in storage account '$destinationSdkUrl'. Skipping copying it..."
    else
        echo
        echo "Blob container '$platformName' does not exist in storage account '$destinationSdkUrl'. Copying it from $PROD_SDK_STORAGE_BASE_URL..."
        # azcopy copy [source] [destination] [flags]
        if [ $dryRun == "False" ] ; then
            "$azCopyDir/azcopy" copy \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" \
                "$destinationSdkUrl/$platformName$sasToken" --overwrite false --recursive
        else
            "$azCopyDir/azcopy" copy \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName$PROD_STORAGE_SAS_TOKEN" \
                "$destinationSdkUrl/$platformName$sasToken" --overwrite false --recursive --dry-run
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

destinationSdkUrl="https://$1.blob.core.windows.net"
sasToken=""
set +x 

# case insensitive matching because both secrets and urls are case insensitive
shopt -s nocasematch
if [[ "$destinationSdkUrl" == $SANDBOX_SDK_STORAGE_BASE_URL ]]; then
    sasToken=$SANDBOX_STORAGE_SAS_TOKEN
elif [[ "$destinationSdkUrl" == $DEV_SDK_STORAGE_BASE_URL ]]; then
    sasToken=$DEV_STORAGE_SAS_TOKEN
elif [[ "$destinationSdkUrl" == $PRIVATE_STAGING_SDK_STORAGE_BASE_URL ]]; then
    sasToken=$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN
elif [[ "$destinationSdkUrl" == $PROD_BACKUP_SDK_STORAGE_BASE_URL ]]; then
    sasToken=$PROD_BACKUP_STORAGE_SAS_TOKEN
# check if the personal sas token has been found in the oryx key vault
elif [[ "$PERSONAL_STORAGE_SAS_TOKEN" != "\$($1-PERSONAL-STORAGE-SAS-TOKEN)" ]]; then 
    sasToken=$PERSONAL_STORAGE_SAS_TOKEN
else
	echo "Error: $destinationSdkUrl is an invalid destination storage account url."
	exit 1
fi
shopt -u nocasematch
set -x

dryRun=$2
if [ $dryRun != "True" ] && [ $dryRun != "False" ]; then
	echo "Error: Dry run must be True or False. Was: '$dryRun'"
	exit 1
fi

overwrite=$3
if [ $overwrite != "True" ] && [ $overwrite != "False" ]; then
	echo "Error: Overwrite must be True or False. Was: '$overwrite'"
	exit 1
fi

copyBlobContainerFromProdToDestination "dotnet"
copyBlobContainerFromProdToDestination "python"
copyBlobContainerFromProdToDestination "nodejs"
copyBlobContainerFromProdToDestination "php"
copyBlobContainerFromProdToDestination "php-composer"
copyBlobContainerFromProdToDestination "java"
copyBlobContainerFromProdToDestination "maven"
copyBlobContainerFromProdToDestination "golang"
