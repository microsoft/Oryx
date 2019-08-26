#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r IMAGE_NAME_PLACEHOLDER="%NODE_BASE_IMAGE%"
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"
declare -r BASE_BUILD_NUMBER="20190826.1"

cd $DIR
for VERSION_DIRECTORY in $(find . -type d -iname '[0-9]*' -printf '%f\n')
do
	echo "Generating Dockerfile for image $VERSION_DIRECTORY..."

	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/node-base:$VERSION_DIRECTORY-$BASE_BUILD_NUMBER"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
done
