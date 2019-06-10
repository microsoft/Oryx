#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

sourceImageName="oryxdevmcr.azurecr.io/public/oryx/build"
#while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  #if [[ $sourceImage == oryxdevmcr.azurecr.io/public/oryx/build:Oryx-CI* ]]; then
    buildNumber=$BUILD_BUILDNUMBER
    sourceImage="$sourceImageName:Oryx-CI.$buildNumber"
    echo "Pulling the source image $sourceImage ..."
    docker pull "$sourceImage" | sed 's/^/     /'
    
		
    acrProdRepo="oryxmcr.azurecr.io/public/oryx/build"
    acrLatest="$acrProdRepo:latest"
    acrSpecific="$acrProdRepo:$buildNumber"
    
    
    dockerHubRepoName="oryxprod/build"
    dockerHubLatest="$dockerHubRepoName:latest"
    dockerHubSpecific="$dockerHubRepoName:$buildNumber"

    echo
    echo "Tagging the source image with tag $acrSpecific..."
    echo "$acrSpecific">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"
    #docker tag "$sourceImage" "$acrSpecific" 
    echo "Tagging the source image with tag $acrLatest..."
    echo "$acrLatest">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"
    #docker tag "$sourceImage" "$acrLatest"

    echo "Tagging the source image with tag $dockerHubLatest..."
    echo "$dockerHubLatest">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-dockerhub.txt"
    #docker tag "$sourceImage" "$dockerHubLatest"
    echo "Tagging the source image with tag $dockerHubSpecific..."
    echo "$dockerHubSpecific">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-dockerhub.txt"
    #docker tag "$sourceImage" "$dockerHubSpecific"
    echo -------------------------------------------------------------------------------
  #fi
#done <"$1"
#done <"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-acr.txt"