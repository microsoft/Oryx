#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh
commit=$(git rev-parse HEAD)
storageAccountName="$1"

uploadFiles() {
    local platform="$1"
    local artifactsDir="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/platformSdks/$platform"
    if [ ! -d "$artifactsDir" ]; then
        return
    fi

    allFiles=$(find $artifactsDir -type f -name '*.tar.gz' -o -name 'defaultVersion.*txt')
    for fileToUpload in $allFiles
    do
        fileName=$(basename $fileToUpload)
        fileNameWithoutExtension=${fileName%".tar.gz"}

        # Check if there is a metadata file for the corresponding tar.gz file and 
        # read and upload it along with the blob
        fileMetadata=""
        metdataFile="$artifactsDir/$fileNameWithoutExtension-metadata.txt"
        if [ -f "$metdataFile" ]; then
            while IFS= read -r line
            do
                line=$(echo $line | tr -d '\r')
                fileMetadata+=" $line"
            done < "$metdataFile"
        fi

        checksum=$(sha512sum $fileToUpload | cut -d " " -f 1)
        if [ "$platform" == "golang" ];then
            checksum=$(sha256sum $fileToUpload | cut -d " " -f 1)
        fi
        
        if shouldOverwriteSdk || shouldOverwritePlatformSdk $platform || [[ "$fileToUpload" == *defaultVersion*txt ]]; then
            az storage blob upload \
            --name $fileName \
            --file "$fileToUpload" \
            --container-name $platform \
            --account-name $storageAccountName \
            --sas-token $sasToken \
            --metadata \
                Buildnumber="$BUILD_BUILDNUMBER" \
                Commit="$commit" \
                Branch="$BUILD_SOURCEBRANCHNAME" \
                Checksum="$checksum" \
                $fileMetadata \
            --overwrite true
        else
            az storage blob upload \
            --name $fileName \
            --file "$fileToUpload" \
            --container-name $platform \
            --account-name $storageAccountName \
            --sas-token $sasToken \
            --metadata \
                Buildnumber="$BUILD_BUILDNUMBER" \
                Commit="$commit" \
                Branch="$BUILD_SOURCEBRANCHNAME" \
                Checksum="$checksum" \
                $fileMetadata
        fi
    done
}

storageAccountUrl="https://$storageAccountName.blob.core.windows.net"
sasToken=""

if [ "$storageAccountUrl" == $SANDBOX_SDK_STORAGE_BASE_URL ]; then
    sasToken=$SANDBOX_STORAGE_SAS_TOKEN
elif [ "$storageAccountUrl" == $DEV_SDK_STORAGE_BASE_URL ]; then
    sasToken=$DEV_STORAGE_SAS_TOKEN
else
	echo "Error: $1 is an invalid destination storage account."
	exit 1
fi

platforms=("nodejs" "python" "dotnet" "php" "php-composer" "ruby" "java" "maven" "golang")
for platform in "${platforms[@]}"
do
    uploadFiles $platform
done 
