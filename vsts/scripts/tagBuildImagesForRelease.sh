#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

function tagBuildImage() {
    local devRegistryImageName="$1"
    local prodRegistryLatestTagName="$2"
    local prodRegistrySpecificTagName="$3"
    local prodRegistryRepoName="oryxmcr.azurecr.io/public/oryx/build"
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    outFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"

    echo "Pulling the source image $devRegistryImageName..."
    docker pull "$devRegistryImageName" | sed 's/^/     /'

    echo
    echo "Tagging the source image with tag $prodRegistrySpecificTagName..."
    prodRegistryImageName="$prodRegistryRepoName:$prodRegistrySpecificTagName"
    docker tag "$devRegistryImageName" "$prodRegistryImageName"
    echo "$prodRegistryImageName">>"$outFileMCR"

    if [ "$sourceBranchName" == "master" ]; then
        echo "Tagging the source image with tag $prodRegistryLatestTagName..."
        prodRegistryImageName="$prodRegistryRepoName:$prodRegistryLatestTagName"
        docker tag "$devRegistryImageName" "$prodRegistryRepoName:$prodRegistryLatestTagName"
        echo "$prodRegistryImageName">>"$outFileMCR"
    else
        echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi

    echo -------------------------------------------------------------------------------
}

tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:Oryx-CI.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/build:slim-Oryx-CI.$RELEASE_TAG_NAME" "slim" "slim-$RELEASE_TAG_NAME"
tagBuildImage "oryxdevmcr.azurecr.io/public/oryx/cli:Oryx-CI.$RELEASE_TAG_NAME" "latest" "$RELEASE_TAG_NAME"