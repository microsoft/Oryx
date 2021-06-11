#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

declare -r outPmeFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxprodmcr-buildpack-images-mcr.txt" 
declare -r sourceImageRepo="oryxdevmcr.azurecr.io/public/oryx"
declare -r prodPmeImageRepo="oryxprodmcr.azurecr.io/public/oryx"

sourceBranchName=$BUILD_SOURCEBRANCHNAME

if [ -f "$outPmeFile" ]; then
    rm $outPmeFile
fi

packImage="$sourceImageRepo/pack:$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
echo "Pulling pack image '$packImage'..."
docker pull "$packImage"
echo "Retagging pack image with '$RELEASE_TAG_NAME'..."

echo "$prodPmeImageRepo/pack:$RELEASE_TAG_NAME">>"$outPmeFile"
docker tag "$packImage" "$prodPmeImageRepo/pack:$RELEASE_TAG_NAME"

if [ "$sourceBranchName" == "main" ]; then
    echo "Retagging pack image with 'stable'..."

    docker tag "$packImage" "$prodPmeImageRepo/pack:stable"
    echo "$prodPmeImageRepo/pack:stable">>"$outPmeFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'main'. Current branch is $sourceBranchName"
fi

echo "printing pme tags from $outPmeFile"
cat $outPmeFile
echo -------------------------------------------------------------------------------