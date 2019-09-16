#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

storageAccount="$1"

uploadFiles() {
    local platform="$1"
    local artifactsDir="$(System.ArtifactsDirectory)/drop/platformSdks/$platform"
    if "ls $artifactsDir/$platform-*.tar.gz" 1> /dev/null 2>&1; then
        az storage blob upload-batch \
            -s "$artifactsDir" \
            -d $platform \
            --account-name $storageAccount
    fi
}

platforms=("node" "python" "dotnet" "php")
for platform in "${platforms[@]}"
do
    uploadFiles $platform
done