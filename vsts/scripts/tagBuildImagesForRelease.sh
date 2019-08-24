#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

buildNumber=$BUILD_BUILDNUMBER

function tagBuildImage() {
    local prodRegistryLatestTagName="$1"
    local prodRegistrySpecificTagName="$2"
    local prodRegistryRepoName="oryxmcr.azurecr.io/public/oryx/build"
    sourceBranchName=$BUILD_SOURCEBRANCHNAME
    buildImageName="oryxdevmcr.azurecr.io/public/oryx/build:Oryx-CI.$buildNumber"
    outFileMCR="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/build-images-mcr.txt"

    echo "Pulling the source image $buildImageName ..."
    docker pull "$buildImageName" | sed 's/^/     /'

    echo
    echo "Tagging the source image with tag $prodRegistrySpecificTagName..."
    docker tag "$buildImageName" "$prodRegistryRepoName:$prodRegistrySpecificTagName"
    echo "$prodRegistryRepoName:$prodRegistrySpecificTagName">>"$outFileMCR"

    if [ "$sourceBranchName" == "master" ]; then
        echo "Tagging the source image with tag $prodRegistryLatestTagName..."
        docker tag "$buildImageName" "$prodRegistryRepoName:$prodRegistryLatestTagName"
        echo "$prodRegistryRepoName:$prodRegistryLatestTagName">>"$outFileMCR"
    else
        echo "Not creating 'latest' tag as source branch is not 'master'. Current branch is $sourceBranchName"
    fi

    echo -------------------------------------------------------------------------------
}

tagBuildImage "latest" "$buildNumber"
tagBuildImage "slim" "slim-$buildNumber"