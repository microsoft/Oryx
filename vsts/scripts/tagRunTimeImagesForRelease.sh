#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage != *:latest ]]; then
    echo "Pulling the source image $sourceImage ..."
    docker pull "$sourceImage" | sed 's/^/     /'
    
    IFS=':'
    read -ra imageNameParts <<< "$sourceImage"
    repo=${imageNameParts[0]}
    tag=${imageNameParts[1]}
    replaceText="Oryx-CI."
    buildNumber=$(echo $tag | sed "s/$replaceText//g")

    IFS='-'
    read -ra repoParts <<< "$repo"
    acrRepoName=${repoParts[0]}
    acrProdRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/oryxmcr/g")
    version=${repoParts[1]}
    acrLatest="$acrProdRepo:$version"
    acrSpecific="$acrProdRepo:$version-$buildNumber"
    
    replaceText="oryxdevmcr.azurecr.io/public/oryx/"
    runtimeName=$(echo $acrRepoName | sed "s!$replaceText!!g")
    dockerHubRepoName="oryxprod/$runtimeName"
    dockerHubLatest="$dockerHubRepoName:$version"
    dockerHubSpecific="$dockerHubRepoName:$version-$buildNumber"

    echo
    echo "Tagging the source image with tag $acrSpecific..."
    echo "$acrSpecific">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-mcr.txt"
    docker tag "$sourceImage" "$acrSpecific" 
    echo "Tagging the source image with tag $acrLatest..."
    echo "$acrLatest">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-mcr.txt"
    docker tag "$sourceImage" "$acrLatest"

    echo "Tagging the source image with tag $dockerHubLatest..."
    echo "$dockerHubLatest">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-dockerhub.txt"
    docker tag "$sourceImage" "$dockerHubLatest"
    echo "Tagging the source image with tag $dockerHubSpecific..."
    echo "$dockerHubSpecific">>"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-dockerhub.txt"
    docker tag "$sourceImage" "$dockerHubSpecific"
    echo -------------------------------------------------------------------------------
  fi
done <"$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-acr.txt"