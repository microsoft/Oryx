#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$DIR/__versions.sh"

declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
declare -r DOCKERFILE_BASE_TEMPLATE="$DIR/template.base.Dockerfile"
declare -r IMAGE_NAME_PLACEHOLDER="%PHP_BASE_IMAGE%"
declare -r PHP_VERSION_PLACEHOLDER="%PHP_VERSION%"
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"

echo "PHP-FPM image type is: $1"
ImageDebianFlavor="$1"

PHP_VERSION_ARRAY=()

if [ "$ImageDebianFlavor" == "bullseye" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BULLSEYE[@]}")
elif [ "$ImageDebianFlavor" == "buster" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BUSTER[@]}")
elif  [ "$ImageDebianFlavor" == "stretch" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY[@]}")
fi

for PHP_VERSION in "${PHP_VERSION_ARRAY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

	PHP_IMAGE_NAME="php-fpm-$VERSION_DIRECTORY"
	echo "Generating Dockerfile for image '$PHP_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$ImageDebianFlavor.Dockerfile"
	TARGET_DOCKERFILE_BASE="$DIR/$VERSION_DIRECTORY/base.$ImageDebianFlavor.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"
	cp "$DOCKERFILE_BASE_TEMPLATE" "$TARGET_DOCKERFILE_BASE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$PHP_IMAGE_NAME|g" "$TARGET_DOCKERFILE_BASE"
	sed -i "s|$PHP_VERSION_PLACEHOLDER|$PHP_VERSION|g" "$TARGET_DOCKERFILE_BASE"

	# RUNTIME_BASE_IMAGE_NAME="php-$VERSION_DIRECTORY-fpm-$PHP_FPM_RUNTIME_BASE_TAG-$ImageDebianFlavor"
    RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:php-$VERSION_DIRECTORY-fpm-$PHP_FPM_RUNTIME_BASE_TAG"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
done
