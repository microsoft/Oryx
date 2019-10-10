#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

declare -r outFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/buildpack-images-mcr.txt" 
declare -r sourceImageRepo="oryxdevmcr.azurecr.io/public/oryx"
declare -r prodImageRepo="oryxmcr.azurecr.io/public/oryx"

sourceBranchName=$BUILD_SOURCEBRANCHNAME

packImage="$sourceImageRepo/pack:Oryx-CI.$RELEASE_TAG_NAME"
echo "Pulling pack image '$packImage'..."
docker pull "$packImage"
echo "Retagging pack image with '$RELEASE_TAG_NAME'..."
echo "$prodImageRepo/pack:$RELEASE_TAG_NAME">>"$outFile"
docker tag "$packImage" "$prodImageRepo/pack:$RELEASE_TAG_NAME"

if [ "$sourceBranchName" == "master" ]; then
    echo "Retagging pack image with 'stable'..."
    docker tag "$packImage" "$prodImageRepo/pack:stable"
    echo "$prodImageRepo/pack:stable">>"$outFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'master'. Current branch is $sourceBranchName"
fi
