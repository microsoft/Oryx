#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

echo "-> Building stack base image: $DOCKER_PACK_STACK_BASE_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_STACK_BASE_IMAGE_DOCKERFILE" \
			 -t $DOCKER_PACK_STACK_BASE_IMAGE_REPO:latest \
			 -t $DOCKER_PACK_BUILDER_IMAGE_REPO:latest \
			 .

cd /tmp

$REPO_DIR/images/pack-builder/install-pack.sh

echo "-> Creating builder image: $DOCKER_PACK_BUILDER_IMAGE_REPO"
echo
./pack create-builder $DOCKER_PACK_BUILDER_IMAGE_REPO:latest \
	--builder-config $REPO_DIR/images/pack-builder/builder.toml \
	--no-pull
echo "-> Created builder"

# Remove pack & everything that was added by it
rm -f   ./pack 
rm -rf ~/.pack

# Build an image that runs `pack`
echo "-> Building pack runner image: $DOCKER_PACK_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_IMAGE_DOCKERFILE" \
	--build-arg BUILD_NUMBER='0.2.0' \
	--build-arg BUILDPACK_BUILDER_NAME="$builderImageName" \
	-t $DOCKER_PACK_IMAGE_REPO:latest \
	.
