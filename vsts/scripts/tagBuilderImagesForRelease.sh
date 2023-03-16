#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -x
set -o pipefail

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

tagBuilderImage "oryxdevmcr.azurecr.io/public/oryx/builder:$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"
tagBuilderImage "oryxdevmcr.azurecr.io/public/oryx/builder:capps-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "capps" "capps-$RELEASE_TAG_NAME"

echo "printing pme tags from $outPmeFileMCR"
cat $outPmeFileMCR
echo -------------------------------------------------------------------------------