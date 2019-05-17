#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r PYTHON_VERSIONS_PATH=$DIR/../../../build/__python-versions.sh
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r PYTHON_MAJOR_VERSION_PLACEHOLDER="%PYTHON_MAJOR_VERSION%"
declare -r PYTHON_VERSION_PLACEHOLDER="%PYTHON_VERSION%"
declare -r PYTHON_FULL_VERSION_PLACEHOLDER="%PYTHON_FULL_VERSION%"
declare -r ORYX_IAMGE_TAG_PLACEHOLDER="%IMAGE_TAG%"
declare -r ORYX_BASE_IMAGE_TAG="20190402.1"

source "$PYTHON_VERSIONS_PATH"
while IFS= read -r PYTHON_VERSION_VAR_NAME || [[ -n $PYTHON_VERSION_VAR_NAME ]]
do
	PYTHON_VERSION=${!PYTHON_VERSION_VAR_NAME}
	IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"
	MAJOR_MINOR_VERSION="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

	mkdir -p "$DIR/$MAJOR_MINOR_VERSION/"
	TARGET_DOCKERFILE="$DIR/$MAJOR_MINOR_VERSION/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$PYTHON_VERSION_PLACEHOLDER|$MAJOR_MINOR_VERSION|g" "$TARGET_DOCKERFILE"
	sed -i "s|$PYTHON_FULL_VERSION_PLACEHOLDER|$PYTHON_VERSION|g" "$TARGET_DOCKERFILE"
	sed -i "s|$PYTHON_MAJOR_VERSION_PLACEHOLDER|${SPLIT_VERSION[0]}|g" "$TARGET_DOCKERFILE"
	sed -i "s|$ORYX_IAMGE_TAG_PLACEHOLDER|$ORYX_BASE_IMAGE_TAG|g" "$TARGET_DOCKERFILE"

done < <(compgen -A variable | grep 'PYTHON[0-9]\{2,\}_VERSION')
