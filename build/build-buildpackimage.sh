#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

packPlatform='linux'
if [[ "$OSTYPE" == "darwin"* ]]; then packPlatform='macos'; fi

wget -nv "https://github.com/buildpack/pack/releases/download/v0.0.9/pack-0.0.9-$packPlatform.tar.gz"
tar -xvf "pack-0.0.9-$packPlatform.tar.gz"
# `pack` is now available for use

./pack create-builder $DOCKER_DEV_REPO_BASE/buildpack-builder --builder-config $REPO_DIR/images/buildpack-builder/builder.toml

#docker build --build-arg BUILD_NUMBER='0.2.0' -f "$BUILDPACK_IMAGE_DOCKERFILE" -t $DOCKER_BUILDPACK_IMAGE_REPO:latest .
