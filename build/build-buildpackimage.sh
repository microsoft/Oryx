#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

docker build --build-arg BUILD_NUMBER='0.2.0' -f "$BUILDPACK_IMAGE_DOCKERFILE" -t $DOCKER_BUILDPACK_IMAGE_REPO:latest .
