#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -x
set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

outPmeFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxprodmcr-build-images-mcr.txt"
outNonPmeFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxmcr-build-images-mcr.txt"

if [ -f "$outPmeFileMCR" ]; then
    rm $outPmeFileMCR
fi

if [ -f "$outNonPmeFileMCR" ]; then
    rm $outNonPmeFileMCR
fi

function tagBuildImage() {
    local devRegistryImageName="$1"
    local prodRegistryLatestTagName="$2"
    local prodRegistrySpecificTagName="$3"
    local prodNonPmeRegistryRepoName="oryxmcr.azurecr.io/public/oryx/build"
    local prodPmeRegistryRepoName="oryxprodmcr.azurecr.io/public/oryx/build"
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    
    echo "Pulling the source image $devRegistryImageName..."
    docker pull "$devRegistryImageName" | sed 's/^/     /'

    echo
    echo "Tagging the source image for $prodPmeRegistryRepoName with tag $prodRegistrySpecificTagName..."
    prodPmeRegistryImageName="$prodPmeRegistryRepoName:$prodRegistrySpecificTagName"
    docker tag "$devRegistryImageName" "$prodPmeRegistryImageName"
    echo "$prodPmeRegistryImageName">>"$outPmeFileMCR"
    
    echo "Tagging the source image for $prodNonPmeRegistryRepoName with tag $prodRegistrySpecificTagName..."
    prodNonPmeRegistryImageName="$prodNonPmeRegistryRepoName:$prodRegistrySpecificTagName"    
    docker tag "$devRegistryImageName" "$prodNonPmeRegistryImageName"
    echo "$prodNonPmeRegistryImageName">>"$outNonPmeFileMCR"

    if [ "$sourceBranchName" == "master" ]; then
        echo "Tagging the source image for $prodPmeRegistryRepoName with tag $prodRegistryLatestTagName..."
        prodPmeRegistryImageName="$prodPmeRegistryRepoName:$prodRegistryLatestTagName"
        docker tag "$devRegistryImageName" "$prodPmeRegistryRepoName:$prodRegistryLatestTagName"
        echo "$prodPmeRegistryImageName">>"$outPmeFileMCR"
        
        echo "Tagging the source image for $prodNonPmeRegistryRepoName with tag $prodRegistryLatestTagName..."
        prodNonPmeRegistryImageName="$prodNonPmeRegistryRepoName:$prodRegistryLatestTagName"
        docker tag "$devRegistryImageName" "$prodNonPmeRegistryRepoName:$prodRegistryLatestTagName"
        echo "$prodNonPmeRegistryImageName">>"$outNonPmeFileMCR"
    else
        echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi
    
    echo -------------------------------------------------------------------------------
}

tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "lts-versions" "lts-versions-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "azfunc-jamstack" "azfunc-jamstack-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "github-actions" "github-actions-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:vso-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "vso" "vso-$RELEASE_TAG_NAME"

echo "printing pme tags from $outPmeFileMCR"
cat $outPmeFileMCR
echo -------------------------------------------------------------------------------
echo "printing non-pme tags from $outNonPmeFileMCR"
cat $outNonPmeFileMCR
echo -------------------------------------------------------------------------------