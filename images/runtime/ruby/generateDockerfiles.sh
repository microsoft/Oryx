#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )

source $REPO_DIR/build/__rubyVersions.sh

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
declare -r RUBY_VERSION_PLACEHOLDER="%RUBY_VERSION%"
declare -r ORYX_IMAGE_TAG_PLACEHOLDER="%IMAGE_TAG%"

declare -r RUBY_STRETCH_VERSION_ARRAY=($RUBY27_VERSION $RUBY26_VERSION)
declare -r RUBY_BUSTER_VERSION_ARRAY=()

ImageDebianFlavor="$1"
echo "ruby baseimage type: $ImageDebianFlavor"

VERSIONS_DIRECTORY=()

if [ "$ImageDebianFlavor" == "buster" ];then
	VERSIONS_DIRECTORY=("${RUBY_BUSTER_VERSION_ARRAY[@]}")
else 
	VERSIONS_DIRECTORY=("${RUBY_STRETCH_VERSION_ARRAY[@]}")
fi

for VERSION_DIRECTORY in "${VERSION_DIRECTORY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$VERSION_DIRECTORY"
    MAJOR_MINOR_VERSION="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    echo "Generating Dockerfile for ruby based images in directory '$MAJOR_MINOR_VERSION'..."

	mkdir -p "$DIR/$MAJOR_MINOR_VERSION/"
	TARGET_DOCKERFILE="$DIR/$MAJOR_MINOR_VERSION/$ImageDebianFlavor.Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$RUBY_VERSION_PLACEHOLDER|$MAJOR_MINOR_VERSION|g" "$TARGET_DOCKERFILE"
	sed -i "s|$ORYX_IMAGE_TAG_PLACEHOLDER|$RUBY_BASE_TAG|g" "$TARGET_DOCKERFILE"
    
done
