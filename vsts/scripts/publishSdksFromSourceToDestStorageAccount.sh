#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

azCopyDir="/tmp/azcopy-tool"

dryRun=$1
if [ $dryRun != "True" ] && [ $dryRun != "False" ]; then
	echo "Error: Dry run must be True or False. Was: '$dryRun'"
	exit 1
fi

export AZCOPY_AUTO_LOGIN_TYPE=AZCLI
export AZCOPY_TENANT_ID=$tenantId

function blobExistsInProd() {
    local containerName="$1"
    local blobName="$2"
    local statusCode
    statusCode=$(curl -s -o /dev/null -w "%{http_code}" -I "$DEST_SDK_STORAGE_BASE_URL/$containerName/$blobName")
    
    if [ "$statusCode" -eq 200 ]; then
        return 0
    else
        return 1
    fi
}

function copyBlob() {
    local platformName="$1"
    local blobName="$2"
    local arg=""

    if shouldOverwriteSdk || shouldOverwritePlatformSdk $platformName || isDefaultVersionFile $blobName; then
        echo
        echo "Blob '$blobName' exists in Prod storage container '$platformName'. Overwriting it..."
        arg=" --overwrite true"
    fi

    if blobExistsInProd $platformName $blobName && [ -z "$arg" ]; then
        echo
        echo "Blob '$blobName' already exists in Prod storage container '$platformName'. Skipping copying it..."
    else
        echo
        echo "Blob '$blobName' does not exist in Prod storage container '$platformName'. Copying it..."
        if [ $dryRun == "False" ]; then
            "$azCopyDir/azcopy" copy \
                "$SOURCE_SDK_STORAGE_BASE_URL/$platformName/$blobName" \
                "$DEST_SDK_STORAGE_BASE_URL/$platformName/$blobName" $arg --from-to BlobBlob --trusted-microsoft-suffixes *.azurefd.net
        else
            "$azCopyDir/azcopy" copy \
                "$SOURCE_SDK_STORAGE_BASE_URL/$platformName/$blobName" \
                "$DEST_SDK_STORAGE_BASE_URL/$platformName/$blobName" --dry-run $arg --from-to BlobBlob --trusted-microsoft-suffixes *.azurefd.net
        fi
    fi
}

function copyPlatformBlobsToProd() {
    local platformName="$1"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "stretch"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "buster"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "bullseye"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "bookworm"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "focal-scm"
    copyPlatformBlobsToProdForOsFlavor "$platformName" "noble"
}

function copyPlatformBlobsToProdForOsFlavor() {
    local platformName="$1"
    local osFlavor="$2"
    local versionsFile="$REPO_DIR/platforms/$platformName/versions/$osFlavor/versionsToBuild.txt"
    local defaultFile=""
    local binaryPrefix=""

    if [ "$platformName" == "php-composer" ]; then
        versionsFile="$REPO_DIR/platforms/php/composer/versions/$osFlavor/versionsToBuild.txt"
    elif [ "$platformName" == "maven" ]; then
        versionsFile="$REPO_DIR/platforms/java/maven/versions/$osFlavor/versionsToBuild.txt"
    fi

    if [ "$osFlavor" == "stretch" ]; then
        defaultFile="defaultVersion.txt"
        copyBlob "$platformName" "$defaultFile"
        binaryPrefix="$platformName"
    else
        binaryPrefix="$platformName-$osFlavor"
    fi

    # Function to copy platform blobs to production for a specific Os flavor
    # Dotnet, nodejs, php and python platforms are currently supporting bookworm.
    # Allowed combinations: 
    # - platformName=dotnet and osFlavor=bookworm
    # - platformName=nodejs and osFlavor=bookworm
    # - platformName=php and osFlavor=bookworm
    # - platformName=python and osFlavor=bookworm
    # - platformName=dotnet and osFlavor=noble
    # - platformName=python and osFlavor=noble
    # Not allowed combinations:
    # - Any platformName other than dotnet, node js, python and php with osFlavor=bookworm
    # - Any platformName other than dotnet, python, node with osFlavor=noble
    if [ "$osFlavor" == "bookworm" ] && \
       [ "$platformName" != "dotnet" ] && \
       [ "$platformName" != "nodejs" ] && \
       [ "$platformName" != "php" ] && \
       [ "$platformName" != "php-composer" ] && \
       [ "$platformName" != "python" ]; then
        # Do not copy blobs
        echo "Copying blobs for platformName=$platformName and osFlavor=$osFlavor is not supported yet."
    elif [ "$osFlavor" == "noble" ] && \
         [ "$platformName" != "dotnet" ] && \
         [ "$platformName" != "nodejs" ] && \
         [ "$platformName" != "python" ]; then
        # Do not copy blobs
        echo "Copying blobs for platformName=$platformName and osFlavor=$osFlavor is not supported yet."
    else
        defaultFile="defaultVersion.$osFlavor.txt"
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
