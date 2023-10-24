#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

azCopyDir="/tmp/azcopy-tool"

dryRun=$1
if [ $dryRun != "True" ] && [ $dryRun != "False" ]; then
	echo "Error: Dry run must be True or False. Was: '$dryRun'"
	exit 1
fi

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
    set +x
    local platformName="$1"
    local blobName="$2"
    local isDefaultVersionFile="$3"

    if shouldOverwriteSdk || shouldOverwritePlatformSdk $platformName || isDefaultVersionFile $blobName; then
        echo
        echo "Blob '$blobName' exists in Prod storage container '$platformName'. Overwriting it..."
        if [ $dryRun == "False" ]; then
            "$azCopyDir/azcopy" copy \
                "$PRIVATE_STAGING_SDK_STORAGE_BASE_URL/$platformName/$blobName$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN" \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN" --overwrite true
        else
            "$azCopyDir/azcopy" copy \
                "$PRIVATE_STAGING_SDK_STORAGE_BASE_URL/$platformName/$blobName$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN" \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN" --overwrite true --dry-run
        fi
    elif blobExistsInProd $platformName $blobName; then
        echo
        echo "Blob '$blobName' already exists in Prod storage container '$platformName'. Skipping copying it..."
    else
        echo
        echo "Blob '$blobName' does not exist in Prod storage container '$platformName'. Copying it..."
        if [ $dryRun == "False" ]; then
            "$azCopyDir/azcopy" copy \
                "$PRIVATE_STAGING_SDK_STORAGE_BASE_URL/$platformName/$blobName$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN" \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN"
        else
            "$azCopyDir/azcopy" copy \
                "$PRIVATE_STAGING_SDK_STORAGE_BASE_URL/$platformName/$blobName$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN" \
                "$PROD_SDK_STORAGE_BASE_URL/$platformName/$blobName$PROD_STORAGE_SAS_TOKEN" --dry-run
        fi
    fi
    set -x
}

function copyPlatformBlobsToProd() {
    local platformName="$1"
    copyPlatformBlobsToProdForDebianFlavor "$platformName" "stretch"
    copyPlatformBlobsToProdForDebianFlavor "$platformName" "buster"
    copyPlatformBlobsToProdForDebianFlavor "$platformName" "bullseye"
    copyPlatformBlobsToProdForDebianFlavor "$platformName" "bookworm"
    copyPlatformBlobsToProdForDebianFlavor "$platformName" "focal-scm"
}

function copyPlatformBlobsToProdForDebianFlavor() {
    local platformName="$1"
    local debianFlavor="$2"
    local versionsFile="$REPO_DIR/platforms/$platformName/versions/$debianFlavor/versionsToBuild.txt"
    local defaultFile=""
    local binaryPrefix=""

    if [ "$platformName" == "php-composer" ]; then
        versionsFile="$REPO_DIR/platforms/php/composer/versions/$debianFlavor/versionsToBuild.txt"
    elif [ "$platformName" == "maven" ]; then
        versionsFile="$REPO_DIR/platforms/java/maven/versions/$debianFlavor/versionsToBuild.txt"
    fi

    if [ "$debianFlavor" == "stretch" ]; then
        defaultFile="defaultVersion.txt"
        copyBlob "$platformName" "$defaultFile"
        binaryPrefix="$platformName"
    else
        binaryPrefix="$platformName-$debianFlavor"
    fi

    # Function to copy platform blobs to production for a specific Debian flavor
    # Dotnet is currently the only platform supporting bookworm.
    # Allowed combinations: 
    # - platformName=dotnet and debianFlavor=bookworm
    # Not allowed combinations: 
    # - platformName=python and debianFlavor=bookworm
    # - platformName=nodejs and debianFlavor=bookworm
    # - platformName=java and debianFlavor=bookworm
    # - Any platformName other than dotnet and debianFlavor=bookworm
    if [ "$platformName" != "dotnet" ] && [ "$debianFlavor" == "bookworm" ]; then
        # Do not copy blobs
        echo "Copying blobs for platformName=$platformName and debianFlavor=$debianFlavor is not supported yet."
    else
        defaultFile="defaultVersion.$debianFlavor.txt"
        copyBlob "$platformName" "$defaultFile"
        
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
            copyBlob "$platformName" "$binaryPrefix-$version.tar.gz"
        done 3< "$versionsFile"
    fi
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
