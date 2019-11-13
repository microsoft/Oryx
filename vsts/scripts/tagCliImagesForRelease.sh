#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

declare -r outFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/cli-images-mcr.txt"
declare -r sourceImageRepo="oryxdevmcr.azurecr.io/public/oryx"
declare -r prodImageRepo="oryxmcr.azurecr.io/public/oryx"

sourceBranchName=$BUILD_SOURCEBRANCHNAME

cliImage="$sourceImageRepo/cli:Oryx-CI.$RELEASE_TAG_NAME"
echo "Pulling CLI image '$cliImage'..."
docker pull "$cliImage"
echo "Retagging CLI image with '$RELEASE_TAG_NAME'..."
echo "$prodImageRepo/cli:$RELEASE_TAG_NAME">>"$outFile"
docker tag "$cliImage" "$prodImageRepo/cli:$RELEASE_TAG_NAME"

if [ "$sourceBranchName" == "master" ]; then
    echo "Retagging CLI image with 'stable'..."
    docker tag "$cliImage" "$prodImageRepo/cli:stable"
    echo "$prodImageRepo/cli:stable">>"$outFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'master'. Current branch is $sourceBranchName"
fi