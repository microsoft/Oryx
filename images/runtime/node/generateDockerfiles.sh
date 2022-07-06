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
declare -r NODE_BULLSEYE_VERSION_ARRAY=($NODE16_VERSION $NODE14_VERSION)

echo "$1"

cd $DIR
if [ "$1" == "bullseye" ];then
	for NODE_BULLSEYE_VERSION in "${NODE_BULLSEYE_VERSION_ARRAY[@]}"
	do
		IFS='.' read -ra SPLIT_VERSION <<< "$NODE_BULLSEYE_VERSION"
		VERSION_DIRECTORY="${SPLIT_VERSION[0]}"

		echo "Generating Dockerfile for bullseye based image $VERSION_DIRECTORY..."

		TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$1.Dockerfile"
		cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

		echo "Generating Dockerfile for bullseye based images..."
		# Replace placeholders
		RUNTIME_BASE_IMAGE_TAG="node-$VERSION_DIRECTORY-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER|$RUNTIME_BASE_IMAGE_TAG|g" "$TARGET_DOCKERFILE"
	done
fi