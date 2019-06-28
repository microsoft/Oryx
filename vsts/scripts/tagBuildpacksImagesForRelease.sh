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

echo "Pulling pack-builder image..."
packBuilderImage=$sourceImageRepo/pack-builder:Oryx-CI.$buildNumber
docker pull "$packBuilderImage"
echo "Retagging pack-builder image 'builderNumber'..."
echo "$prodImageRepo/pack-builder:$buildNumber">>"$outFile"
docker tag "$packBuilderImage" "$prodImageRepo/pack-builder:$buildNumber"

echo "Pulling pack-stack-base..."
packStackBaseImage=$sourceImageRepo/pack-stack-base:Oryx-CI.$buildNumber
docker pull "$packStackBaseImage"
echo "Retagging pack-stack-base image 'builderNumber'..."
echo "$prodImageRepo/pack-stack-base:$buildNumber">>"$outFile"
docker tag "$packStackBaseImage" "$prodImageRepo/pack-stack-base:$buildNumber"

echo "Pulling pack image..."
packImage=$sourceImageRepo/pack:Oryx-CI.$buildNumber
docker pull "$packImage"
echo "Retagging pack image with 'builderNumber'..."
echo "$prodImageRepo/pack:$buildNumber">>"$outFile"
docker tag "$packImage" "$prodImageRepo/pack:$buildNumber"

if [ "$sourceBranchName" == "master" ]; then
    echo "Retagging pack-builder image with 'stable'..."
    docker tag "$packBuilderImage" "$prodImageRepo/pack-builder:stable"
    echo "$prodImageRepo/pack-builder:stable">>"$outFile"

    echo "Retagging pack-stack-base image with 'latest'..."
    docker tag "$packStackBaseImage" "$prodImageRepo/pack-stack-base:latest"
    echo "$prodImageRepo/pack-stack-base:latest">>"$outFile"

    echo "Retagging pack image with 'stable'..."
    docker tag "$packImage" "$prodImageRepo/pack:stable"
    echo "$prodImageRepo/pack:stable">>"$outFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'master'. Current branch is $sourceBranchName"
fi