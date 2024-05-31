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

buildImageTagFilter=""

if [ -n "$TESTINTEGRATIONCASEFILTER" ];then
    IFS='&'
    read -a splitArr <<< "$TESTINTEGRATIONCASEFILTER"
    for val in "${splitArr[@]}";
    do
        if [[ "$val" == "build-image="* ]];then
            buildImagePrefix="build-image="
            strippedVal=${val#"$buildImagePrefix"}
            buildImageTagFilter="$strippedVal"
        fi
    done
fi

if [ -n "$buildImageTagFilter" ];then
    echo
    echo "Filtering test build images by provided build image tag filter '$buildImageTagFilter'"
fi

echo
echo "Building build images for tests..."


# Build GitHub Actions buster build image
if [ -z "$buildImageTagFilter" ] || [ "$buildImageTagFilter" == "github-actions-debian-buster" ];then
    echo "Building buster based GitHub Action image for tests..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-buster" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-buster \
        -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses buster based github action as a base but doesn't have all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-buster-base" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-buster \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses buster based github action as a base and has all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-buster-base-withenv" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-buster \
        --build-arg DEBIAN_FLAVOR=buster \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_WITHENV_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo
fi

# Build GitHub Actions bullseye build image and helper build images
if [ -z "$buildImageTagFilter" ] || [ "$buildImageTagFilter" == "github-actions-debian-bullseye" ];then
    echo "Building bullseye based GitHub Action image for tests..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bullseye" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bullseye \
        -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses bullseye based GitHub Action as a base but doesn't have all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bullseye-base" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bullseye \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses bullseye based GitHub Action as a base and has all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bullseye-base-withenv" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bullseye \
        --build-arg DEBIAN_FLAVOR=bullseye \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_WITHENV_BUILDIMAGE_DOCKERFILE" \
        .

    echo
fi

# Build GitHub Actions bookworm build image and helper build images
if [ -z "$buildImageTagFilter" ] || [ "$buildImageTagFilter" == "github-actions-debian-bookworm" ];then
    echo "Building bookworm based GitHub Action image for tests..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bookworm" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bookworm \
        -f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses bookworm based GitHub Action as a base but doesn't have all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bookworm-base" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bookworm \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_BUILDIMAGE_DOCKERFILE" \
        .

    echo
    echo

    echo "Building image that uses bookworm based GitHub Action as a base and has all required environment variables..."
    docker build \
        -t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions-debian-bookworm-base-withenv" \
        --build-arg PARENT_IMAGE_BASE=github-actions-debian-bookworm \
        --build-arg DEBIAN_FLAVOR=bookworm \
        -f "$ORYXTESTS_GITHUB_ACTIONS_ASBASE_WITHENV_BUILDIMAGE_DOCKERFILE" \
        .

    echo
fi

dockerCleanupIfRequested
