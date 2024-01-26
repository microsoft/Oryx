#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r artifactsDir="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images"
declare -r outFileName="base-images-mcr.txt"
declare -r buildNumber=$BUILD_BUILDNUMBER
 
function retagImageWithStagingRepository()
{
    echo "Number of arguments passed: $@"
    echo "Pulling and retagging bases images from '$1'..."

    # '$1' is the imagetag file name
    # '$2' is the image name e.g dotnetcore
    # '$3' argument is the debianflavor e.g buster or stretch

    echo "Reading file '$1' to pull images from dev acr ..."
    echo "Retagging images for image '$3' based '$2' for registry '$acrProdRepoPrefix'..."
    
    local artifactsFile="$artifactsDir/$1"
    local outFile="$artifactsDir/$2/$acrProdRepoPrefix-$3-$outFileName"

    echo "output tags to be written to: '$outFile'"

    while read sourceImage; do
    # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
    if [[ $sourceImage != *:latest ]]; then
        echo "Pulling the source image $sourceImage ..."
        docker pull "$sourceImage" | sed 's/^/     /'
        
        # Following is an example of released tag for this image 
        # build-yarn-cache:20190621.3 

        newtag=$(echo "$sourceImage" | sed -r 's/oryxdevmcr/'"$acrProdRepoPrefix"'/')
        echo
        echo "Tagging the source image with tag $newtag ..."
        echo "$newtag">>"$outFile"
        docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
        echo -------------------------------------------------------------------------------
    fi
    done <"$artifactsFile"
}

# first argument to the script is the image name e.g dotnetcore
# second argument to the script is the acrrepoprefix, e.g. oryxmcr

imageName=$1
acrProdRepoPrefix="$2"

if [[ -z $imageName ]]; then
  echo
  echo "Invalid parameter: imageName cann't be blank"
  exit 1
fi  

echo "Creating release tags for '$imageName' ..."
mkdir -p $artifactsDir/$imageName

if [ "$imageName" == "node" ]
then
  echo ""
  retagImageWithStagingRepository node-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository node-runtimeimage-bases-bullseye.txt $imageName bullseye
  retagImageWithStagingRepository node-runtimeimage-bases-bookworm.txt $imageName bookworm
elif [ "$imageName" == "python-build" ]
then
  echo ""
  retagImageWithStagingRepository python-buildimage-bases.txt $imageName $acrProdRepoPrefix
elif [ "$imageName" == "python" ]
then
  echo ""
  echo $imageName
  retagImageWithStagingRepository python-runtimeimage-bases-bullseye.txt $imageName bullseye
  retagImageWithStagingRepository python-runtimeimage-bases-bookworm.txt $imageName bookworm
elif [ "$imageName" == "php-build" ]
then
  echo ""
  retagImageWithStagingRepository php-buildimage-bases.txt $imageName $acrProdRepoPrefix
elif [ "$imageName" == "php" ]
then
  echo ""
  retagImageWithStagingRepository php-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository php-runtimeimage-bases-bullseye.txt $imageName bullseye
elif [ "$imageName" == "php-fpm" ]
then
  echo ""
  echo $imageName
  retagImageWithStagingRepository php-fpm-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository php-fpm-runtimeimage-bases-bullseye.txt $imageName bullseye
elif [ "$imageName" == "dotnetcore" ]
then
  echo ""
  echo $imageName
  retagImageWithStagingRepository dotnetcore-runtimeimage-bases-buster.txt $imageName buster
  retagImageWithStagingRepository dotnetcore-runtimeimage-bases-bullseye.txt $imageName bullseye
  retagImageWithStagingRepository dotnetcore-runtimeimage-bases-bookworm.txt $imageName bookworm
elif [ "$imageName" == "ruby" ]
then
  echo ""
  echo $imageName
  retagImageWithStagingRepository ruby-runtimeimage-bases-buster.txt $imageName buster
else
  echo "ImageName $imageName is invalid/not supported.. "
  exit 1
fi