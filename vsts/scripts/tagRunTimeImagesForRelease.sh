#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

# $1 >> non pme production staging repository i.e. oryxmcr
# $2 >> pme production staging repository i.e. oryxprodmcr

acrNonPmeProdRepo=$1
acrPmeProdRepo=$2

sourceBranchName=$BUILD_SOURCEBRANCHNAME
outFileNonPmeMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/$acrNonPmeProdRepo-runtime-images-mcr.txt"
outFilePmeMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/$acrPmeProdRepo-runtime-images-mcr.txt"
sourceFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/runtime-images-acr.txt"

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage == *:*-* ]]; then
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
    releaseTagName=$(echo $tag | sed "s/$replaceText//g")

    IFS='-'
    read -ra tagParts <<< "$tag"
    version="${tagParts[0]}"

    read -ra repoParts <<< "$repo"
    acrRepoName=${repoParts[0]}
    acrProdNonPmeRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/'"$acrNonPmeProdRepo"'/g")
    acrProdPmeRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/'"$acrPmeProdRepo"'/g")
    acrNonPmeLatest="$acrProdNonPmeRepo:$version"
    acrNonPmeSpecific="$acrProdNonPmeRepo:$releaseTagName"
    acrPmeLatest="$acrProdPmeRepo:$version"
    acrPmeSpecific="$acrProdPmeRepo:$releaseTagName"

    echo
    echo "Tagging the source image with tag $acrNonPmeSpecific and $acrPmeSpecific..."
    echo "$acrNonPmeSpecific">>"$outFileNonPmeMCR"
    echo "$acrPmeSpecific">>"$outFilePmeMCR"
    docker tag "$sourceImage" "$acrNonPmeSpecific"
    docker tag "$sourceImage" "$acrPmeSpecific"

    if [ "$sourceBranchName" == "master" ]; then
      echo "Tagging the source image with tag $acrLatest and $acrPmeLatest..."
      echo "$acrNonPmeLatest">>"$outFileNonPmeMCR"
      echo "$acrPmeLatest">>"$outFilePmeMCR"
      docker tag "$sourceImage" "$acrNonPmeLatest"
      docker tag "$sourceImage" "$acrPmeLatest"
    else
      echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi
    echo -------------------------------------------------------------------------------
  fi
done <"$sourceFile"