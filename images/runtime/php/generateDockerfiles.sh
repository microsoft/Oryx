#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r DOCKERFILE_BASE_TEMPLATE="$DIR/Dockerfile.base.template"
declare -r IMAGE_NAME_PLACEHOLDER="%PHP_BASE_IMAGE%"
declare -r PHP_VERSION_PLACEHOLDER="%PHP_VERSION%"
. $DIR/../../../build/__phpVersions.sh
declare -r VERSION_ARRAY=($PHP73_VERSION $PHP72_VERSION $PHP70_VERSION $PHP56_VERSION)
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"

for PHP_VERSION in "${VERSION_ARRAY[@]}"
do
	PHP_IMAGE_NAME="$PHP_VERSION-apache"

	IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
	echo "Generating Dockerfile for image '$PHP_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	TARGET_DOCKERFILE_BASE="$DIR/$VERSION_DIRECTORY/Dockerfile.base"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"
	cp "$DOCKERFILE_BASE_TEMPLATE" "$TARGET_DOCKERFILE_BASE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$PHP_IMAGE_NAME|g" "$TARGET_DOCKERFILE_BASE"
	sed -i "s|$PHP_VERSION_PLACEHOLDER|$PHP_VERSION|g" "$TARGET_DOCKERFILE_BASE"

	RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/php-base:$VERSION_DIRECTORY-$PHP_RUNTIME_BASE_TAG"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
done
