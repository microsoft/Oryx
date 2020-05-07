#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )

source $REPO_DIR/build/__nodeVersions.sh

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE_BUSTER="$DIR/template.Buster.Dockerfile"
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BUSTER_BASE_IMAGE_NAME%"

cd $DIR
for VERSION_DIRECTORY in $(find . -type d -iname '[0-9]*' -printf '%f\n')
do
	echo "Generating Dockerfile for image $VERSION_DIRECTORY..."

	TARGET_BUSTER_DOCKERFILE="$DIR/$VERSION_DIRECTORY/buster.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE_BUSTER" "$TARGET_BUSTER_DOCKERFILE"

	echo "Generating Dockerfile for buster images..."
	# Replace placeholders
	RUNTIME_BASE_IMAGE_NAME_BUSTER="mcr.microsoft.com/oryx/base:node-$VERSION_DIRECTORY-$NODE_RUNTIME_BASE_TAG-buster"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME_BUSTER|g" "$TARGET_BUSTER_DOCKERFILE"
done