#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r VERSIONS_FILE="$DIR/nodeVersions.txt"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r DOCKERFILE_BASE_TEMPLATE="$DIR/Dockerfile.base.template"
declare -r IMAGE_NAME_PLACEHOLDER="%NODE_BASE_IMAGE%"
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"

# Example line:
# 8.11.4-stretch
while IFS= read -r NODE_IMAGE_NAME || [[ -n $NODE_IMAGE_NAME ]]
do
	IFS='.' read -ra SPLIT_VERSION <<< "$NODE_IMAGE_NAME"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
	echo "Generating Dockerfile for image '$NODE_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	TARGET_DOCKERFILE_BASE="$DIR/$VERSION_DIRECTORY/Dockerfile.base"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"
	cp "$DOCKERFILE_BASE_TEMPLATE" "$TARGET_DOCKERFILE_BASE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$NODE_IMAGE_NAME|g" "$TARGET_DOCKERFILE_BASE"

	RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/node-$VERSION_DIRECTORY-base:20190514.1"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
done < "$VERSIONS_FILE"