#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -exo pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

outPmeFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxprodmcr-builder-images-mcr.txt"

if [ -f "$outPmeFileMCR" ]; then
    rm $outPmeFileMCR
fi

function tagBuilderImage() {
    local devRegistryImageName="$1"
    local prodRegistryLatestTagName="$2"
    local prodRegistrySpecificTagName="$3"
    local prodPmeRegistryRepoName="oryxprodmcr.azurecr.io/public/oryx/builder"
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    
    echo "Pulling the source image $devRegistryImageName..."
    docker pull "$devRegistryImageName" | sed 's/^/     /'

    echo
    echo "Tagging the source image for $prodPmeRegistryRepoName with tag $prodRegistrySpecificTagName..."
    prodPmeRegistryImageName="$prodPmeRegistryRepoName:$prodRegistrySpecificTagName"
    docker tag "$devRegistryImageName" "$prodPmeRegistryImageName"
    echo "$prodPmeRegistryImageName">>"$outPmeFileMCR"
    
    if [ "$sourceBranchName" == "main" ]; then
        echo "Tagging the source image for $prodPmeRegistryRepoName with tag $prodRegistryLatestTagName..."
        prodPmeRegistryImageName="$prodPmeRegistryRepoName:$prodRegistryLatestTagName"
        docker tag "$devRegistryImageName" "$prodPmeRegistryRepoName:$prodRegistryLatestTagName"
        echo "$prodPmeRegistryImageName">>"$outPmeFileMCR"
        
    else
        echo "Not creating 'latest' tag as source branch is not 'main'. Current branch is $sourceBranchName"
    fi
    
    echo -------------------------------------------------------------------------------
}

tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"
tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:capps-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "capps" "capps-$RELEASE_TAG_NAME"
tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:buildpack-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "buildpack" "buildpack-$RELEASE_TAG_NAME"
tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:stack-base-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "stack-base" "stack-base-$RELEASE_TAG_NAME"
tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:stack-build-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "stack-build" "stack-build-$RELEASE_TAG_NAME"
tagBuilderImage "$ACR_PUBLIC_PREFIX/builder:stack-run-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "stack-run" "stack-run-$RELEASE_TAG_NAME"

echo "printing pme tags from $outPmeFileMCR"
cat $outPmeFileMCR
echo -------------------------------------------------------------------------------