#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

if [ -n "$BUILD_BUILDIMAGES_USING_NOCACHE" ]; then
	echo
	echo "Building buildpacks image(s) with NO cache..."
	noCacheFlag='--no-cache'
else
	echo
	echo "Building buildpacks image(s)..."
fi

echo "-> Building stack base image: $DOCKER_PACK_STACK_BASE_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_STACK_BASE_IMAGE_DOCKERFILE" $noCacheFlag \
			 -t "$DOCKER_PACK_STACK_BASE_IMAGE_REPO:latest" \
			 -t "mcr.microsoft.com/oryx/$DOCKER_PACK_STACK_BASE_IMAGE_NAME" \
			 .

cd /tmp

$REPO_DIR/images/pack-builder/install-pack.sh

echo "-> Creating builder image: $DOCKER_PACK_STACK_BASE_IMAGE_REPO"
echo
./pack create-builder $DOCKER_PACK_BUILDER_IMAGE_REPO \
					  --builder-config $REPO_DIR/images/pack-builder/builder.toml \
					  --no-pull

builderFqn="mcr.microsoft.com/oryx/$DOCKER_PACK_BUILDER_IMAGE_NAME"
docker tag "$DOCKER_PACK_BUILDER_IMAGE_REPO" "$builderFqn"

# Remove pack & everything that was added by it
rm -f   ./pack
rm -rf ~/.pack

# Build an image that runs `pack`
echo "-> Building pack runner image: $DOCKER_PACK_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_IMAGE_DOCKERFILE" $noCacheFlag \
			 --build-arg BUILD_NUMBER="$BUILD_NUMBER" \
			 --build-arg DEFAULT_BUILDER_NAME="$builderFqn" \
			 -t $DOCKER_PACK_IMAGE_REPO:latest \
			 .
