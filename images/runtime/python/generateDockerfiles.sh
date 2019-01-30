#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r VERSIONS_FILE="$DIR/pythonVersions.txt"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r IMAGE_NAME_PLACEHOLDER="%PYTHON_BASE_IMAGE%"
declare -r ALPINE_OR_STRETCH_PLACEHOLDER="%PYTHON_BASE_IMAGE_ALPINE_OR_STRETCH%"

# Example line:
# 8.11.4-alpine
while IFS= read -r PYTHON_IMAGE_NAME || [[ -n $PYTHON_IMAGE_NAME ]]
do
	IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_IMAGE_NAME"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
	echo "Generating Dockerfile for image '$PYTHON_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."

	GO_IMAGE_TYPE="stretch"
	# Figure out if the final image is Alpine based
	if [[ $PYTHON_IMAGE_NAME == *"alpine"* ]]; then
		GO_IMAGE_TYPE="alpine"
	fi

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$PYTHON_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	sed -i "s|$ALPINE_OR_STRETCH_PLACEHOLDER|$GO_IMAGE_TYPE|g" "$TARGET_DOCKERFILE"

done < "$VERSIONS_FILE"