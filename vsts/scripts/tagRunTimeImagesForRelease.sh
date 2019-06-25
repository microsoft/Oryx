#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

outFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-mcr.txt"
outFileDocker="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-mcr.txt"
sourceFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-acr.txt"

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage != *:latest ]]; then
    echo "Pulling the source image $sourceImage ..."
    docker pull "$sourceImage" | sed 's/^/     /'
    
    # We tag out runtime images in dev differently than in tag. In dev we have builddefnitionname as part of tag. 
    # We don't want that in our prod tag. Also we want versions (like node-10.10:latest to be tagged as 
    # node:10.10-latest) as part of tag. We need to parse the tags so that we can reconstruct tags suitable for our 
    # prod images.
    
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
    echo "$acrSpecific">>"$outFileMCR"
    docker tag "$sourceImage" "$acrSpecific" 
    echo "Tagging the source image with tag $acrLatest..."
    echo "$acrLatest">>"$outFileMCR"
    docker tag "$sourceImage" "$acrLatest"

    echo "Tagging the source image with tag $dockerHubLatest..."
    echo "$dockerHubLatest">>"$outFileDocker"
    docker tag "$sourceImage" "$dockerHubLatest"
    echo "Tagging the source image with tag $dockerHubSpecific..."
    echo "$dockerHubSpecific">>"$outFileDocker"
    docker tag "$sourceImage" "$dockerHubSpecific"
    echo -------------------------------------------------------------------------------
  fi
done <"$sourceFile"