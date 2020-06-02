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
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"
declare -r NODE_BUSTER_VERSION_ARRAY=($NODE14_VERSION)

echo "$1"

cd $DIR
if [ "$1" == "buster" ];then
	for NODE_BUSTER_VERSION in "${NODE_BUSTER_VERSION_ARRAY[@]}"
	do
		IFS='.' read -ra SPLIT_VERSION <<< "$NODE_BUSTER_VERSION"
		VERSION_DIRECTORY="${SPLIT_VERSION[0]}"

		echo "Generating Dockerfile for buster based image $VERSION_DIRECTORY..."

		TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$1.Dockerfile"
		cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

		echo "Generating Dockerfile for buster based images..."
		# Replace placeholders
		RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$1-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	done
else
	for VERSION_DIRECTORY in $(find . -type d -iname '[0-9]*' -printf '%f\n')
	do
		echo "Generating Dockerfile for stretch based image $VERSION_DIRECTORY..."

		TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$1.Dockerfile"
		cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

		echo "Generating Dockerfile for stretch based images..."
		# Replace placeholders
		RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$1-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	done
fi