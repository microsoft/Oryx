#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r outFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/buildpack-images-mcr.txt" 
declare -r sourceImageRepo="oryxdevmcr.azurecr.io/public/oryx"
declare -r prodImageRepo="oryxmcr.azurecr.io/public/oryx"

buildNumber=$BUILD_BUILDNUMBER
sourceBranchName=$BUILD_SOURCEBRANCHNAME

echo "Pulling pack image..."
packImage=$sourceImageRepo/pack:Oryx-CI.$buildNumber
docker pull "$packImage"
echo "Retagging pack image with 'buildNumber'..."
echo "$prodImageRepo/pack:$buildNumber">>"$outFile"
docker tag "$packImage" "$prodImageRepo/pack:$buildNumber"

if [ "$sourceBranchName" == "master" ]; then
    echo "Retagging pack image with 'stable'..."
    docker tag "$packImage" "$prodImageRepo/pack:stable"
    echo "$prodImageRepo/pack:stable">>"$outFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'master'. Current branch is $sourceBranchName"
fi
