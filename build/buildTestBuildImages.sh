#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

echo
echo "Building build images for tests..."


echo "Building buster based GitHub Action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-buster" \
    --build-arg PARENT_IMAGE_BASE=$ORYX_TEST_IMAGE_BASE:github-actions-debian-buster-$IMAGE_BUILDNUMBER \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building bullseye based GitHub Action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bullseye" \
    --build-arg PARENT_IMAGE_BASE=$ORYX_TEST_IMAGE_BASE:github-actions-debian-bullseye-$IMAGE_BUILDNUMBER \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

echo "Building bookworm based GitHub Action image for tests..."
docker build \
    -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bookworm" \
    --build-arg PARENT_IMAGE_BASE=$ORYX_TEST_IMAGE_BASE:github-actions-debian-bookworm-$IMAGE_BUILDNUMBER \
    -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
    .

echo
echo

dockerCleanupIfRequested
