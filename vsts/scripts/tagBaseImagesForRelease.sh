#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r artifactsDir="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images"
declare -r outFileName="base-images-mcr.txt" 
declare -r acrProdRepo="oryxmcr.azurecr.io/public/oryx"
declare -r buildNumber=$BUILD_BUILDNUMBER
 
function retagImageWithStagingRepository()
{
    echo "Number of arguments passed: $@"
    echo "Pulling and retagging bases images for '$1'..."
    
    baseImageDebianFlavor="$3"

    echo "base image type: '$3'"

    local artifactsFile="$artifactsDir/$1"
    local outFile="$artifactsDir/$2/$3-$outFileName"

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

imageName=$1

if [[ -z $imageName ]]; then
  echo
  echo "Invalid parameter: imageName cann't be blank"
  exit 1
fi  

echo "Creating release tags for '$imageName' ..."
mkdir -p $artifactsDir/$imageName

if [ "$imageName" == "yarn-cache-build" ]
then
  echo ""
  retagImageWithStagingRepository yarn-cache-buildimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository yarn-cache-buildimage-bases-stretch.txt $imageName stretch
elif [ "$imageName" == "node" ]
then
  echo ""
  retagImageWithStagingRepository node-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository node-runtimeimage-bases-stretch.txt $imageName stretch
elif [ "$imageName" == "python-build" ]
then
  echo ""
  retagImageWithStagingRepository python-buildimage-bases.txt $imageName
elif [ "$imageName" == "php-build" ]
then
  echo ""
  retagImageWithStagingRepository php-buildimage-bases.txt $imageName
elif [ "$imageName" == "php" ]
then
  echo ""
  retagImageWithStagingRepository php-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository php-runtimeimage-bases-stretch.txt $imageName stretch
elif [ "$imageName" == "php-fpm" ]
then
  echo ""
  echo $imageName
  retagImageWithStagingRepository php-fpm-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository php-fpm-runtimeimage-bases-stretch.txt $imageName stretch
elif [ "$imageName" == "dotnetcore" ]
then
  echo ""
  retagImageWithStagingRepository dotnetcore-runtimeimage-bases.txt $imageName
else
  echo "ImageName $imageName is invalid/not supported.. "
  exit 1
fi