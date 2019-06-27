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

echo "Pulling pack-builder image and retagging with 'stable' and 'builderNumber'..."
packBuilderImage=$sourceImageRepo/pack-builder:Oryx-CI.$buildNumber
docker pull "$packBuilderImage"
echo "$prodImageRepo/pack-builder:stable">>"$outFile"
echo "$prodImageRepo/pack-builder:$buildNumber">>"$outFile"
docker tag "$packBuilderImage" "$prodImageRepo/pack-builder:stable"
docker tag "$packBuilderImage" "$prodImageRepo/pack-builder:$buildNumber"

echo "Pulling pack-stack-base image and retagging with 'latest' and 'builderNumber'..."
packStackBaseImage=$sourceImageRepo/pack-stack-base:Oryx-CI.$buildNumber
docker pull "$packStackBaseImage"
echo "$prodImageRepo/pack-stack-base:latest">>"$outFile"
echo "$prodImageRepo/pack-stack-base:$buildNumber">>"$outFile"
docker tag "$packStackBaseImage" "$prodImageRepo/pack-stack-base:latest"
docker tag "$packStackBaseImage" "$prodImageRepo/pack-stack-base:$buildNumber"

echo "Pulling pack image and retagging with 'stable' and 'builderNumber'..."
packImage=$sourceImageRepo/pack:Oryx-CI.$buildNumber
docker pull "$packImage"
echo "$prodImageRepo/pack:stable">>"$outFile"
echo "$prodImageRepo/pack:$buildNumber">>"$outFile"
docker tag "$packImage" "$prodImageRepo/pack:stable"
docker tag "$packImage" "$prodImageRepo/pack:$buildNumber"
