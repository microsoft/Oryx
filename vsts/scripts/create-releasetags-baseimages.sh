#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r filePath="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images"
declare -r outFileName="base-images-mcr.txt" 

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
        
        newtag=$(echo "$sourceImage" | sed -r 's/oryxdevmcr/oryxmcr/')
        echo
        echo "Tagging the source image with tag $newtag ..."
        echo "$newtag">>"$outFile"
        docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
        echo -------------------------------------------------------------------------------
    fi
    done <"$artifactsFile"
}

function retagNodeRuntimeBaseImages()
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
        
        IFS=':'
        read -ra imageNameParts <<< "$sourceImage"
        repo=${imageNameParts[0]}
        tag=${imageNameParts[1]}
        replaceText="Oryx-BaseImages."
        buildNumber=$(echo $tag | sed "s/$replaceText//g")

        IFS='-'
        read -ra repoParts <<< "$repo"
        acrRepoName=${repoParts[0]}
        acrImageName=${repoParts[1]}
        imageType=${repoParts[2]}
        
        acrProdRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/oryxmcr/g")
        acrProdRepo="$acrProdRepo-$imageType"
        echo "prod acr name: "$acrProdRepo
        version=${repoParts[1]}
        acrLatest="$acrProdRepo:$version"
        acrSpecific="$acrProdRepo:$version-$buildNumber"
        
        echo "acr latest tag: $acrLatest"
        echo "acr specific tag: $acrSpecific"
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

#retagNodeRuntimeBaseImages node-runtimeimage-bases.txt

function retagPythonBaseImages()
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
        
        IFS=':'
        read -ra imageNameParts <<< "$sourceImage"
        repo=${imageNameParts[0]}
        tag=${imageNameParts[1]}
        replaceText="Oryx-BaseImages."
        buildNumber=$(echo $tag | sed "s/$replaceText//g")

        IFS='-'
        read -ra repoParts <<< "$repo"
        acrRepoName=${repoParts[0]}
        acrImageName=${repoParts[1]}
        imageVersion=${repoParts[2]}
        acrProdRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/oryxmcr/g")
        acrProdRepo="$acrProdRepo-$acrImageName-base"
        
        version="$imageVersion"
        acrLatest="$acrProdRepo:$version"
        acrSpecific="$acrProdRepo:$version-$buildNumber"

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


function retagPHPBuildBaseImages()
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
        
        IFS=':'
        read -ra imageNameParts <<< "$sourceImage"
        repo=${imageNameParts[0]}
        tag=${imageNameParts[1]}
        replaceText="Oryx-BaseImages."
        buildNumber=$(echo $tag | sed "s/$replaceText//g")

        IFS='-'
        read -ra repoParts <<< "$repo"
        acrRepoName=${repoParts[0]}
        acrImageName=${repoParts[1]}
        imageVersion=${repoParts[2]}
        acrProdRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/oryxmcr/g")
        acrProdRepo="$acrProdRepo-$acrImageName-base"
        
        version="$imageVersion"
        acrLatest="$acrProdRepo:$version"
        acrSpecific="$acrProdRepo:$version-$buildNumber"

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
#pullAndRetagImages yarn-cache-buildimage-bases.txt

function retagPHPRuntimeBaseImages()
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
        
        IFS=':'
        read -ra imageNameParts <<< "$sourceImage"
        repo=${imageNameParts[0]}
        tag=${imageNameParts[1]}
        replaceText="Oryx-BaseImages."
        buildNumber=$(echo $tag | sed "s/$replaceText//g")

        IFS='-'
        read -ra repoParts <<< "$repo"
        acrRepoName=${repoParts[0]}
        acrImageName=${repoParts[1]}
        imageType=${repoParts[2]}
        
        acrProdRepo=$(echo $acrRepoName | sed "s/oryxdevmcr/oryxmcr/g")
        acrProdRepo="$acrProdRepo-$imageType"
        echo "prod acr name: "$acrProdRepo
        version=${repoParts[1]}
        acrLatest="$acrProdRepo:$version"
        acrSpecific="$acrProdRepo:$version-$buildNumber"
        
        echo "acr latest tag: $acrLatest"
        echo "acr specific tag: $acrSpecific"
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

if [ $imageName == 'yarn-cache-build' ]
then
  echo ""
  retagYarnCacheImage yarn-cache-buildimage-bases.txt $imageName
elif [ $imageName == 'node' ]
then
  echo ""
  retagNodeRuntimeBaseImages node-runtimeimage-bases.txt $imageName
elif [ $imageName == 'python-build' ]
then
  echo ""
  retagPythonBaseImages python-buildimage-bases.txt $imageName
elif [ $imageName == 'php-build' ]
then
  echo ""
  retagPHPBuildBaseImages php-buildimage-bases.txt $imageName
elif [ $imageName == 'php' ]
then
  echo ""
  retagPHPRuntimeBaseImages php-runtimeimage-bases.txt $imageName
else
  echo "ImageName $imageName is invalid/not supported.. "
  exit 1
fi