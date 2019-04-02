#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

cd /tmp

$REPO_DIR/images/buildpack-builder/install-pack.sh

builderName="$DOCKER_DEV_REPO_BASE/buildpack-builder"

./pack create-builder $builderName --builder-config $REPO_DIR/images/buildpack-builder/builder.toml \
															  --stack com.microsoft.oryx.stack --no-pull

rm -rf ~/.pack

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build --build-arg BUILD_NUMBER='0.2.0' --build-arg BUILDPACK_BUILDER_NAME="$builderName" \
			 -f "$PACK_IMAGE_DOCKERFILE" -t $DOCKER_PACK_IMAGE_REPO:latest .
