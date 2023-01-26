#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

declare -r outPmeFile="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxprodmcr-cli-images-mcr.txt"
declare -r sourceImageRepo="oryxdevmcr.azurecr.io/public/oryx"
declare -r prodPmeImageRepo="oryxprodmcr.azurecr.io/public/oryx"

sourceBranchName=$BUILD_SOURCEBRANCHNAME

if [ -f "$outPmeFile" ]; then
    rm $outPmeFile
fi

cliImage="$sourceImageRepo/cli:debian-stretch-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
cliBusterImage="$sourceImageRepo/cli:debian-buster-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
cliBullseyeImage="$sourceImageRepo/cli:debian-bullseye-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
echo "Pulling CLI image '$cliImage'..."
docker pull "$cliImage"

echo "Retagging CLI image for $prodPmeImageRepo with 'debian-stretch-$RELEASE_TAG_NAME'..."
echo "$prodPmeImageRepo/cli:debian-stretch-$RELEASE_TAG_NAME">>"$outPmeFile"
docker tag "$cliImage" "$prodPmeImageRepo/cli:debian-stretch-$RELEASE_TAG_NAME"

echo "Pulling CLI buster image '$cliBusterImage'..."
docker pull "$cliBusterImage"

echo "Retagging CLI buster image for $prodPmeImageRepo with 'debian-buster-$RELEASE_TAG_NAME'..."
echo "$prodPmeImageRepo/cli:debian-buster-$RELEASE_TAG_NAME">>"$outPmeFile"
docker tag "$cliBusterImage" "$prodPmeImageRepo/cli:debian-buster-$RELEASE_TAG_NAME"

echo "Pulling CLI bullseye image '$cliBullseyeImage'"
docker pull "$cliBullseyeImage"

echo "Retagging CLI bullseye image for $prodPmeImageRepo with 'debian-bullseye-$RELEASE_TAG_NAME'..."
echo "$prodPmeImageRepo/cli:debian-bullseye-$RELEASE_TAG_NAME">>"$outPmeFile"
docker tag "$cliBullseyeImage" "$prodPmeImageRepo/cli:debian-bullseye-$RELEASE_TAG_NAME"

if [ "$sourceBranchName" == "main" ]; then
    echo "Retagging CLI image with '{os type}-stable'..."

    docker tag "$cliImage" "$prodPmeImageRepo/cli:debian-stretch-stable"
    echo "$prodPmeImageRepo/cli:debian-stretch-stable">>"$outPmeFile"

    docker tag "$cliBusterImage" "$prodPmeImageRepo/cli:debian-buster-stable"
    echo "$prodPmeImageRepo/cli:debian-buster-stable">>"$outPmeFile"

    docker tag "$cliBullseyeImage" "$prodPmeImageRepo/cli:debian-bullseye-stable"
    echo "$prodPmeImageRepo/cli:debian-bullseye-stable">>"$outPmeFile"
else
    echo "Not creating 'stable' or 'latest' tags as source branch is not 'main'. Current branch is $sourceBranchName"
fi

echo "printing pme tags from $outPmeFile"
cat $outPmeFile
echo -------------------------------------------------------------------------------