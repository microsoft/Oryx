#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
#
# This script builds some base images that are needed for the build image:
# - Python binaries
# - PHP binaries
#

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

ImageDebianFlavor="$2"

# Example: dontetcore, python
IMAGE_DIR_TO_BUILD="$1"
UNIQUE_TAG=""
if [ ! -z "$BUILD_NUMBER" ]; then
	UNIQUE_TAG="-$BUILD_NUMBER"
fi

BUILD_IMAGES_DIR="$REPO_DIR/images/build"
# NOTE: We create a unique artifacts file per image directory here since they are going to be built in parallel on CI
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$IMAGE_DIR_TO_BUILD-buildimage-bases-$ImageDebianFlavor.txt"

# Clean artifacts
mkdir -p `dirname $ARTIFACTS_FILE`
> $ARTIFACTS_FILE

function buildImages() {
	local dirName="$1"
	local dockerFiles=$(find "$BUILD_IMAGES_DIR/$dirName" -type f -name "Dockerfile")
	for dockerFile in $dockerFiles; do
		versionDir=$(dirname "${dockerFile}")
		versionDirName=$(basename $versionDir)
		imageName="$BASE_IMAGES_PUBLIC_REPO:$dirName-build-$versionDirName$UNIQUE_TAG"
		docker build -f $dockerFile -t "$imageName" $REPO_DIR
		echo "$imageName" >> $ARTIFACTS_FILE
	done
}

case $IMAGE_DIR_TO_BUILD in
	'python')
		echo "Building Python base images"
		echo

		docker build -f $BUILD_IMAGES_DIR/python/prereqs/Dockerfile -t "oryxdevmcr.azurecr.io/private/oryx/python-build-prereqs" $REPO_DIR
		buildImages "python"
		;;
	*) echo "Unknown image directory";;
esac

echo
echo "List of images built (from '$ARTIFACTS_FILE'):"
cat $ARTIFACTS_FILE
echo
