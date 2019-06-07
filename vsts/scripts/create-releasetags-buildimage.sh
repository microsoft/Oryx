#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage == oryxdevmcr.azurecr.io/public/oryx/build* ]]; then
    echo "Pulling the source image $sourceImage ..."
    docker pull "$sourceImage" | sed 's/^/     /'
    
    #buildNumber="20190606.01"
		buildNumber=$(Build.BuildNumber)
    acrProdRepo="oryxmcr.azurecr.io/public/oryx/build"
    acrLatest="$acrProdRepo:latest"
    acrSpecific="$acrProdRepo:$buildNumber"
    
    
    dockerHubRepoName="oryxprod/build"
    dockerHubLatest="$dockerHubRepoName:latest"
    dockerHubSpecific="$dockerHubRepoName:$buildNumber"

    echo
    echo "Tagging the source image with tag $acrSpecific..."
    echo "$acrSpecific">>"$(Build.ArtifactStagingDirectory)/drop/images/build-images-mcr.txt"
    docker tag "$sourceImage" "$acrSpecific" 
    echo "Tagging the source image with tag $acrLatest..."
    echo "$acrLatest">>"$(Build.ArtifactStagingDirectory)/drop/images/build-images-mcr.txt"
    docker tag "$sourceImage" "$acrLatest"

    echo "Tagging the source image with tag $dockerHubLatest..."
    echo "$dockerHubLatest">>"$(Build.ArtifactStagingDirectory)/drop/images/build-images-dockerhub.txt"
    docker tag "$sourceImage" "$dockerHubLatest"
    echo "Tagging the source image with tag $dockerHubSpecific..."
    echo "$dockerHubSpecific">>"$(Build.ArtifactStagingDirectory)/drop/images/build-images-dockerhub.txt"
    docker tag "$sourceImage" "$dockerHubSpecific"
    echo -------------------------------------------------------------------------------
  fi
#done <"$1"
done <"$(Build.StagingDirectory)/drop/images/build-images-acr.txt"