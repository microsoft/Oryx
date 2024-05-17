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
declare -r IMAGE_TAG_PLACEHOLDER="%PHP_BASE_IMAGE_TAG%"
declare -r PHP_VERSION_PLACEHOLDER="%PHP_VERSION%"
declare -r RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER="%RUNTIME_BASE_IMAGE_TAG%"

echo "PHP-FPM image type is: $1"
ImageDebianFlavor="$1"

PHP_VERSION_ARRAY=()

if [ "$ImageDebianFlavor" == "bookworm" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BOOKWORM[@]}")
elif [ "$ImageDebianFlavor" == "bullseye" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BULLSEYE[@]}")
elif [ "$ImageDebianFlavor" == "buster" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BUSTER[@]}")
fi

for PHP_VERSION in "${PHP_VERSION_ARRAY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

	PHP_IMAGE_TAG="php-fpm-$VERSION_DIRECTORY-$ImageDebianFlavor"
	echo "Generating Dockerfile with tag '$PHP_IMAGE_TAG' in directory '$VERSION_DIRECTORY'..."

	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/$ImageDebianFlavor.Dockerfile"
	TARGET_DOCKERFILE_BASE="$DIR/$VERSION_DIRECTORY/base.$ImageDebianFlavor.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"
	cp "$DOCKERFILE_BASE_TEMPLATE" "$TARGET_DOCKERFILE_BASE"

	# Replace placeholders
	sed -i "s|$IMAGE_TAG_PLACEHOLDER|$PHP_IMAGE_TAG|g" "$TARGET_DOCKERFILE_BASE"
	sed -i "s|$PHP_VERSION_PLACEHOLDER|$PHP_VERSION|g" "$TARGET_DOCKERFILE_BASE"

	# RUNTIME_BASE_IMAGE_TAG="php-$VERSION_DIRECTORY-fpm-$PHP_FPM_RUNTIME_BASE_TAG-$ImageDebianFlavor"
    RUNTIME_BASE_IMAGE_TAG="php-$VERSION_DIRECTORY-fpm-debian-$ImageDebianFlavor-$PHP_FPM_RUNTIME_BASE_TAG"
	sed -i "s|$RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER|$RUNTIME_BASE_IMAGE_TAG|g" "$TARGET_DOCKERFILE"
done
