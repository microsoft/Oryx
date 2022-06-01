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
		RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	done
elif [ "$1" == "buster" ];then
	for NODE_BUSTER_VERSION in "${NODE_BUSTER_VERSION_ARRAY[@]}"
	do
		IFS='.' read -ra SPLIT_VERSION <<< "$NODE_BUSTER_VERSION"
		VERSION_DIRECTORY="${SPLIT_VERSION[0]}"

		echo "Generating Dockerfile for buster based image $VERSION_DIRECTORY..."

		TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$1.Dockerfile"
		cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

		echo "Generating Dockerfile for buster based images..."
		# Replace placeholders
		RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	done
elif [ "$1" == "stretch" ];then
	dockerFiles=$(find . -type f \( -name "base.stretch.Dockerfile" \) )
	for dockerFile in $dockerFiles; do
		dockerFileDir=$(dirname "${dockerFile}")
		echo "docker file dir: "$dockerFileDir
		IFS=/ read -ra SPLIT_VERSION <<< "$dockerFileDir"
		VERSION_DIRECTORY="${SPLIT_VERSION[1]}"
		
		echo "Generating Dockerfile for stretch based image $VERSION_DIRECTORY..."
		echo "version directory is: $VERSION_DIRECTORY"

		TARGET_DOCKERFILE="$DIR/$dockerFileDir/$1.Dockerfile"
		cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

		echo "Generating Dockerfile for stretch based images..."
		# Replace placeholders
		RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$NODE_RUNTIME_BASE_TAG"
		sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"	
	done
fi