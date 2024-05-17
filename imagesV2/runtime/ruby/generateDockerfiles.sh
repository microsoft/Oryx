#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r RUBY_VERSIONS_PATH=$DIR/../../../build/__rubyVersions.sh
declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
# Ruby major version, e.g. '2', '3'
declare -r RUBY_MAJOR_VERSION_PLACEHOLDER="%RUBY_MAJOR_VERSION%"
# Ruby version as we usually refer to, e.g. '2.7', '2.6'
declare -r RUBY_VERSION_PLACEHOLDER="%RUBY_VERSION%"
# Ruby full version, including patch, e.g. '2.7.1'
declare -r RUBY_FULL_VERSION_PLACEHOLDER="%RUBY_FULL_VERSION%"
declare -r ORYX_IMAGE_TAG_PLACEHOLDER="%IMAGE_TAG%"

source "$RUBY_VERSIONS_PATH"

# Please make sure that any changes to debian flavors supported here are also reflected in build/constants.yaml
declare -r RUBY_BOOKWORM_VERSION_ARRAY=()
declare -r RUBY_BULLSEYE_VERSION_ARRAY=($RUBY27_VERSION $RUBY26_VERSION $RUBY25_VERSION)
declare -r RUBY_BUSTER_VERSION_ARRAY=($RUBY27_VERSION $RUBY26_VERSION $RUBY25_VERSION)

ImageDebianFlavor="$1"
echo "ruby baseimage type: $ImageDebianFlavor"

VERSIONS_DIRECTORY=()

if [ "$ImageDebianFlavor" == "bookworm" ];then
	VERSIONS_DIRECTORY=("${RUBY_BOOKWORM_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "bullseye" ];then
	VERSIONS_DIRECTORY=("${RUBY_BULLSEYE_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "buster" ];then
	VERSIONS_DIRECTORY=("${RUBY_BUSTER_VERSION_ARRAY[@]}")
fi

for VERSION_DIRECTORY in "${VERSIONS_DIRECTORY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$VERSION_DIRECTORY"
    MAJOR_MINOR_VERSION="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    echo "Generating Dockerfile for ruby based images in directory '$MAJOR_MINOR_VERSION'..."

	mkdir -p "$DIR/$MAJOR_MINOR_VERSION/"
	TARGET_DOCKERFILE="$DIR/$MAJOR_MINOR_VERSION/$ImageDebianFlavor.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$RUBY_VERSION_PLACEHOLDER|$MAJOR_MINOR_VERSION|g" "$TARGET_DOCKERFILE"
	sed -i "s|$RUBY_FULL_VERSION_PLACEHOLDER|$VERSION_DIRECTORY|g" "$TARGET_DOCKERFILE"
	sed -i "s|$RUBY_MAJOR_VERSION_PLACEHOLDER|${SPLIT_VERSION[0]}|g" "$TARGET_DOCKERFILE"
	sed -i "s|$ORYX_IMAGE_TAG_PLACEHOLDER|$RUBY_BASE_TAG|g" "$TARGET_DOCKERFILE"

done