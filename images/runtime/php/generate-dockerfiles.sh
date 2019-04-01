#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r IMAGE_NAME_PLACEHOLDER="%PHP_BASE_IMAGE%"
declare -r PHP_VERSION_PLACEHOLDER="%PHP_VERSION%"
. $DIR/../../../build/__php-versions.sh
declare -r VERSION_ARRAY=($PHP73_VERSION $PHP72_VERSION $PHP70_VERSION $PHP56_VERSION)

for PHP_VERSION in "${VERSION_ARRAY[@]}"
do
	PHP_IMAGE_NAME="$PHP_VERSION-apache"

	IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
	echo "Generating Dockerfile for image '$PHP_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$PHP_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	sed -i "s|$PHP_VERSION_PLACEHOLDER|$PHP_VERSION|g" "$TARGET_DOCKERFILE"
done
