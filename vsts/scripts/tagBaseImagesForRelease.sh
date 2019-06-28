#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r filePath="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images"
declare -r outFileName="base-images-mcr.txt" 
declare -r acrProdRepo="oryxmcr.azurecr.io/public/oryx"
declare -r buildNumber=$BUILD_BUILDNUMBER

function retagYarnCacheImage()
{
    echo "Pulling and retagging bases images for '$1'..."
    
    local artifactsFile="$filePath/$1"
    local outFile="$filePath/$2/$outFileName"

    echo "output tags to be written to: '$outFile'"

    while read sourceImage; do
    # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
    if [[ $sourceImage != *:latest ]]; then
        echo "Pulling the source image $sourceImage ..."
        docker pull "$sourceImage" | sed 's/^/     /'
        
        # Following is an example of released tag for this image 
        # build-yarn-cache:20190621.3 

        newtag=$(echo "$sourceImage" | sed -r 's/oryxdevmcr/oryxmcr/')
        echo
        echo "Tagging the source image with tag $newtag ..."
        echo "$newtag">>"$outFile"
        docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
        echo -------------------------------------------------------------------------------
    fi
    done <"$artifactsFile"
}

function retagBaseImages()
{
    echo "Pulling and retagging bases images for '$1'..."

    local artifactsFile="$filePath/$1"
    local outFile="$filePath/$2/$outFileName"

    echo "output tags to be written to: '$outFile'"

    while read sourceImage; do
    # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
    if [[ $sourceImage != *:latest ]]; then
        echo "Pulling the source image $sourceImage ..."
        docker pull "$sourceImage" | sed 's/^/     /'
        
        # We tag out runtime images in dev differently than in tag. In dev we have build defnitionname as part 
        # of the tag. We don't want that in our prod tag. Also we want versions (like node-10.10:latest to be 
        # tagged as node:10.10-latest) as part of tag. We need to parse the tags so that we can reconstruct tags 
        # suitable for our prod images. Following are some examples:
        # node-6.2-base:20190621.3 >> node-base:6.2-20190621.3 'base image for node runtime'
        # php-build-5.6:20190621.3 >> php-build-base:5.6-20190621.3 'php base image for build'
        # python-build-5.6:20190621.3 >> python-build-base:5.6-20190621.3 'python base image for both build & runtime'
        # php-7.3-base:20190621.3 >> php-base:7.3-20190621.3 'php base image runtime'

        IFS=':'
        read -ra imageNameParts <<< "$sourceImage"
        repo=${imageNameParts[0]}
        tag=${imageNameParts[1]}
        replaceText="Oryx-BaseImages."

        IFS='-'
        read -ra repoParts <<< "$repo"
        acrRepoName=${repoParts[0]}
        acrImageName=${repoParts[1]}
        imageVersion=${repoParts[2]}

        acrProdImage="$acrProdRepo/$2-base"
        if [ "$2" == "python-build" ] || [ "$2" == "php-build" ]
        then
          version="$imageVersion"
        else
          version="$acrImageName"
        fi
        
        acrLatest="$acrProdImage:$version"
        acrSpecific="$acrProdImage:$version-$buildNumber"

        echo
        echo "Tagging the source image with tag $acrSpecific "
        echo "$acrSpecific">>"$outFile"
        docker tag "$sourceImage" "$acrSpecific"
        echo "Tagging the source image with tag $acrLatest "
        docker tag "$sourceImage" "$acrLatest"
        echo "$acrLatest">>"$outFile"
        echo -------------------------------------------------------------------------------
    fi
    done <"$artifactsFile"
}

imageName=$1

if [[ -z $imageName ]]; then
  echo
  echo "Invalid parameter: imageName cann't be blank"
  exit 1
fi  

echo "Creating release tags for '$imageName' ..."
mkdir -p $filePath/$imageName
ls -l $filePath

if [ "$imageName" == "yarn-cache-build" ]
then
  echo ""
  retagYarnCacheImage yarn-cache-buildimage-bases.txt $imageName
elif [ "$imageName" == "node" ]
then
  echo ""
  retagBaseImages node-runtimeimage-bases.txt $imageName
elif [ "$imageName" == "python-build" ]
then
  echo ""
  retagBaseImages python-buildimage-bases.txt $imageName
elif [ "$imageName" == "php-build" ]
then
  echo ""
  retagBaseImages php-buildimage-bases.txt $imageName
elif [ "$imageName" == "php" ]
then
  echo ""
  retagBaseImages php-runtimeimage-bases.txt $imageName
else
  echo "ImageName $imageName is invalid/not supported.. "
  exit 1
fi