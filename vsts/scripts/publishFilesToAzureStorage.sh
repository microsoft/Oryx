#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh
commit=$GIT_COMMIT
storageAccountName="$1"
afdHostname="$2"

uploadFiles() {
    local platform="$1"
    local artifactsDir="$ARTIFACTS_DIR/platformSdks/$platform"
    if [ ! -d "$artifactsDir" ]; then
        return
    fi

    allFiles=$(find $artifactsDir -type f -name '*.tar.gz' -o -name 'defaultVersion.*txt')
    for fileToUpload in $allFiles
    do
        echo "Uploading $fileToUpload to $platform"
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

        # Generate a time-limited SAS token for the upload ---
        # This step uses your existing service connection to authorize the creation of the SAS token.
        # The token will be valid for 10 minutes.

        echo "🔄 Generating SAS token..."
        expiryDate=$(date -u -d "30 minutes" '+%Y-%m-%dT%H:%MZ')

        sasToken=$(az storage blob generate-sas \
            --account-name $storageAccountName \
            --container-name $platform \
            --name $fileName \
            --permissions "racw" \
            --expiry $expiryDate \
            --auth-mode login \
            -o tsv)

        if [ -z "$sasToken" ]; then
            echo "❌ Failed to generate SAS token."
            exit 1
        fi
        
        if shouldOverwriteSdk || shouldOverwritePlatformSdk $platform || [[ "$fileToUpload" == *defaultVersion*txt ]]; then          
            echo "running az command with override"
            az storage blob upload \
            --name $fileName \
            --file "$fileToUpload" \
            --container-name $platform \
            --account-name $storageAccountName \
            --metadata \
                Buildnumber="$BUILD_BUILDNUMBER" \
                Commit="$commit" \
                Branch="$BUILD_SOURCEBRANCHNAME" \
                Checksum="$checksum" \
                $fileMetadata \
            --overwrite true
        else
            echo "Uploading $fileName to AFD..."
            uploadUrl="https://$afdHostname/$platform/$fileName?$sasToken"

            curl -X PUT -T "$fileToUpload" \
            --header "x-ms-blob-type: BlockBlob" \
            --header "x-ms-meta-Buildnumber: $buildNumber" \
            --header "x-ms-meta-Commit: $commit" \
            --header "x-ms-meta-Branch: $branch" \
            --header "x-ms-meta-Checksum: $checksum" \
            "$uploadUrl"

            if [ $? -ne 0 ]; then
                echo "❌ Failed to upload $fileName to AFD."
                exit 1
            else
                echo "✅ Successfully uploaded $fileName to AFD."
            fi
        fi
    done
}

storageAccountUrl="https://$storageAccountName.blob.core.windows.net"

platforms=("nodejs" "python" "dotnet" "php" "php-composer" "ruby" "java" "maven" "golang")
for platform in "${platforms[@]}"
do
    uploadFiles $platform
done 
