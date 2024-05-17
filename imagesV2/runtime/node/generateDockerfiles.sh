#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )

source $REPO_DIR/build/__nodeVersions.sh

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
declare -r RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER="%RUNTIME_BASE_IMAGE_TAG%"

# Please make sure that any changes to debian flavors supported here are also reflected in build/constants.yaml
declare -r NODE_BOOKWORM_VERSION_ARRAY=($NODE20_VERSION)
declare -r NODE_BULLSEYE_VERSION_ARRAY=($NODE20_VERSION $NODE18_VERSION $NODE16_VERSION $NODE14_VERSION)
declare -r NODE_BUSTER_VERSION_ARRAY=($NODE16_VERSION $NODE14_VERSION)

ImageDebianFlavor="$1"
echo "node baseimage type: $ImageDebianFlavor"

VERSIONS_DIRECTORY=()

if [ "$ImageDebianFlavor" == "bookworm" ];then
    VERSIONS_DIRECTORY=("${NODE_BOOKWORM_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "bullseye" ];then
    VERSIONS_DIRECTORY=("${NODE_BULLSEYE_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "buster" ];then
    VERSIONS_DIRECTORY=("${NODE_BUSTER_VERSION_ARRAY[@]}")
fi

cd $DIR

for NODE_VERSION_DIRECTORY  in "${VERSIONS_DIRECTORY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$NODE_VERSION_DIRECTORY"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}"

	echo "Generating Dockerfile for $ImageDebianFlavor based image $VERSION_DIRECTORY..."

	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$1.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	echo "Generating Dockerfile for $ImageDebianFlavor based images..."
	# Replace placeholders
	RUNTIME_BASE_IMAGE_TAG="node-$VERSION_DIRECTORY-debian-$ImageDebianFlavor-$NODE_RUNTIME_BASE_TAG"
	sed -i "s|$RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER|$RUNTIME_BASE_IMAGE_TAG|g" "$TARGET_DOCKERFILE"
done
