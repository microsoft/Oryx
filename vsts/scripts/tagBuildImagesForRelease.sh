#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

function tagBuildImage() {
    local devRegistryRepoName="$1"
    local prodRegistryRepoName="$1"
    local prodRegistryTagName="$2"
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    buildImageName="$devRegistryRepoName:Oryx-CI.$buildNumber"
    outFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"

    echo "Pulling the source image $buildImageName ..."
    docker pull "$buildImageName" | sed 's/^/     /'
        
    acrLatest="$prodRegistryRepoName:latest"
    acrSpecific="$prodRegistryRepoName:$buildNumber"    

    echo
    echo "Tagging the source image with tag $acrSpecific..."
    echo "$acrSpecific">>"$outFileMCR"
    docker tag "$buildImageName" "$acrSpecific"

    if [ "$sourceBranchName" == "master" ]; then
        echo "Tagging the source image with tag $acrLatest..."
        echo "$acrLatest">>"$outFileMCR"
        docker tag "$buildImageName" "$acrLatest"
    else
        echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi

    echo -------------------------------------------------------------------------------
}

buildNumber=$BUILD_BUILDNUMBER
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build" "oryxmcr.azurecr.io/public/oryx/build" $buildNumber
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build-slim" "oryxmcr.azurecr.io/public/oryx/build-slim" "slim-$buildNumber"