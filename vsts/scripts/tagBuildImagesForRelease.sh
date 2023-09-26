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

if [ -f "$outPmeFileMCR" ]; then
    rm $outPmeFileMCR
fi

function tagBuildImage() {
    local devRegistryImageName="$1"
    local prodRegistryLatestTagName="$2"
    local prodRegistrySpecificTagName="$3"
    local prodPmeRegistryRepoName="oryxprodmcr.azurecr.io/public/oryx/build"
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

tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:debian-stretch-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "debian-stretch" "debian-stretch-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-debian-stretch-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "lts-versions-debian-stretch" "lts-versions-debian-stretch-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-debian-buster-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "lts-versions-debian-buster" "lts-versions-debian-buster-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-stretch-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "azfunc-jamstack-debian-stretch" "azfunc-jamstack-debian-stretch-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-buster-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "azfunc-jamstack-debian-buster" "azfunc-jamstack-debian-buster-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-bullseye-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "azfunc-jamstack-debian-bullseye" "azfunc-jamstack-debian-bullseye-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-stretch-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "github-actions-debian-stretch" "github-actions-debian-stretch-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-buster-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "github-actions-debian-buster" "github-actions-debian-buster-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-bullseye-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "github-actions-debian-bullseye" "github-actions-debian-bullseye-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-bookworm-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "github-actions-debian-bookworm" "github-actions-debian-bookworm-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:vso-ubuntu-focal-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "vso-ubuntu-focal" "vso-ubuntu-focal-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:vso-debian-bullseye-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "vso-debian-bullseye" "vso-debian-bullseye-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:full-debian-buster-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "full-debian-buster" "full-debian-buster-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:full-debian-bullseye-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME" "full-debian-bullseye" "full-debian-bullseye-$RELEASE_TAG_NAME"

echo "printing pme tags from $outPmeFileMCR"
cat $outPmeFileMCR
echo -------------------------------------------------------------------------------