#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__functions.sh

buildImageDebianFlavor="$1"

echo
echo "Building build images for tests..."

echo "Building stretch based github action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-stretch" \
    --build-arg PARENT_IMAGE_BASE=github-actions-stretch \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building buster based github action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-buster" \
    --build-arg PARENT_IMAGE_BASE=github-actions-buster \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building bullseye based github action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-bullseye" \
    --build-arg PARENT_IMAGE_BASE=github-actions-bullseye \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building stretch based full build image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:latest-stretch" \
    -f "$ORYXTESTS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building stretch based lts-version image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:lts-versions-stretch" \
    --build-arg PARENT_IMAGE_BASE=lts-versions-stretch \
    -f "$ORYXTESTS_LTS_VERSIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building buster based lst version image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:lts-versions-buster" \
    --build-arg PARENT_IMAGE_BASE=lts-versions-buster \
    -f "$ORYXTESTS_LTS_VERSIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
dockerCleanupIfRequested
