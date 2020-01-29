#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

commit=$(git rev-parse HEAD)
storageAccount="$1"

uploadFiles() {
    local platform="$1"
    local artifactsDir="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/platformSdks/$platform"
    allFiles=$(find $artifactsDir -type f -name '*.*')
    for fileToUpload in $allFiles
    do
        checksum=$(sha512sum $fileToUpload | cut -d " " -f 1)
        fileName=$(basename $fileToUpload)
        az storage blob upload \
            --name $fileName \
            --file "$fileToUpload" \
            --container-name $platform \
            --account-name $storageAccount \
            --metadata \
                buildnumber="$BUILD_BUILDNUMBER" \
                commit="$commit" \
                branch="$BUILD_SOURCEBRANCHNAME" \
                checksum="$checksum"
    done
}

platforms=("nodejs" "python" "dotnet" "php" "php-composer")
for platform in "${platforms[@]}"
do
    uploadFiles $platform
done 
