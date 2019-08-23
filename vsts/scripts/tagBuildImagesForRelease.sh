#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

function tagBuildImage() {
    local buildImageRepo="$1"
    buildNumber=$BUILD_BUILDNUMBER
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    buildImageName="$buildImageRepo:Oryx-CI.$buildNumber"
    outFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"
    outFileDocker="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-dockerhub.txt"

    echo "Pulling the source image $buildImageName ..."
    docker pull "$buildImageName" | sed 's/^/     /'
        
    acrProdRepo="oryxmcr.azurecr.io/public/oryx/build"
    acrLatest="$acrProdRepo:latest"
    acrSpecific="$acrProdRepo:$buildNumber"    
        
    dockerHubRepoName="oryxprod/build"
    dockerHubLatest="$dockerHubRepoName:latest"
    dockerHubSpecific="$dockerHubRepoName:$buildNumber"

    echo
    echo "Tagging the source image with tag $acrSpecific..."
    echo "$acrSpecific">>"$outFileMCR"
    docker tag "$buildImageName" "$acrSpecific"

    echo "Tagging the source image with tag $dockerHubSpecific..."
    echo "$dockerHubSpecific">>"$outFileDocker"
    docker tag "$buildImageName" "$dockerHubSpecific"

    if [ "$sourceBranchName" == "master" ]; then
        echo "Tagging the source image with tag $acrLatest..."
        echo "$acrLatest">>"$outFileMCR"
        docker tag "$buildImageName" "$acrLatest"

        echo "Tagging the source image with tag $dockerHubLatest..."
        echo "$dockerHubLatest">>"$outFileDocker"
        docker tag "$buildImageName" "$dockerHubLatest"
    else
        echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi

    echo -------------------------------------------------------------------------------
}

tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build-slim"