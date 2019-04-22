#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

# Enables running from within other scripts that already declared $REPO_DIR
if [ -z "$REPO_DIR" ]; then
	declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
fi

# Enables running from within other scripts that already sourced __variables.sh
if [ -z "$__REPO_DIR" ]; then
	source $REPO_DIR/build/__variables.sh
fi

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

# Even though the image isn't pushed to MCR yet,
# its final name needs to be baked into the `pack` runner image ($PACK_IMAGE_DOCKERFILE)
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

if [ -n "$BUILD_NUMBER" ]; then
	docker tag "$DOCKER_PACK_BUILDER_IMAGE_REPO" "$DOCKER_PACK_BUILDER_IMAGE_REPO:$BUILD_DEFINITIONNAME.$BUILD_NUMBER"
	echo "$DOCKER_PACK_BUILDER_IMAGE_REPO:$BUILD_DEFINITIONNAME.$BUILD_NUMBER" >> $BUILD_IMAGES_ARTIFACTS_FILE
	docker tag "$DOCKER_PACK_IMAGE_REPO:latest" "$DOCKER_PACK_IMAGE_REPO:$BUILD_DEFINITIONNAME.$BUILD_NUMBER"
	echo "$DOCKER_PACK_IMAGE_REPO:$BUILD_DEFINITIONNAME.$BUILD_NUMBER" >> $BUILD_IMAGES_ARTIFACTS_FILE
fi
