#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly DOCKERFILE="$REPO_DIR/images/runtime/python/embr/template.Dockerfile"
readonly ACR_NAME="${ACR_NAME:-oryxdevmcr.azurecr.io}"
readonly ACR_REPOSITORY="${ACR_REPOSITORY:-public/oryx/python}"
readonly RELEASE_TAG_NAME="${RELEASE_TAG_NAME:-local}"
readonly BUILD_DEFINITIONNAME="${BUILD_DEFINITIONNAME:-local}"
readonly ARTIFACTS_DIR="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$REPO_DIR/artifacts}"
readonly IMAGE_LIST="$ARTIFACTS_DIR/images/embr-python-runtime-images-acr.resolute.txt"
readonly PYTHON_VERSIONS="${EMBR_PYTHON_VERSIONS:-3.13 3.14 3.15}"
readonly OS_FLAVOR="resolute"
readonly BASE_IMAGE="${BASE_IMAGE:-mcr.microsoft.com/mirror/docker/library/ubuntu:resolute}"
readonly PIP_INDEX_URL="${PIP_INDEX_URL:-https://pypi.org/simple}"
export PIP_INDEX_URL

mkdir -p "$(dirname "$IMAGE_LIST")"
: > "$IMAGE_LIST"

for minor_version in $PYTHON_VERSIONS; do
    metadata="$(bash "$REPO_DIR/build/getEmbrPythonVersionMetadata.sh" "$minor_version")"
    IFS='|' read -r full_version gpg_keys sha256 <<< "$metadata"

    image="$ACR_NAME/$ACR_REPOSITORY:embr-$minor_version-ubuntu-resolute-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
    docker build \
        --file "$DOCKERFILE" \
        --tag "$image" \
        --secret id=pip_index_url,env=PIP_INDEX_URL \
        --build-arg "OS_FLAVOR=$OS_FLAVOR" \
        --build-arg "BASE_IMAGE=$BASE_IMAGE" \
        --build-arg "PYTHON_FULL_VERSION=$full_version" \
        --build-arg "PYTHON_VERSION=$minor_version" \
        --build-arg "PYTHON_MAJOR_VERSION=3" \
        --build-arg "PYTHON_SHA256=$sha256" \
        --build-arg "PYTHON_GPG_KEYS=$gpg_keys" \
        --build-arg "BUILD_NUMBER=${BUILD_BUILDNUMBER:-local}" \
        --build-arg "GIT_COMMIT=${BUILD_SOURCEVERSION:-unspecified}" \
        --build-arg "RELEASE_TAG_NAME=$RELEASE_TAG_NAME" \
        "$REPO_DIR"
    echo "$image" >> "$IMAGE_LIST"
done

echo "Built Embr Python runtime images:"
cat "$IMAGE_LIST"
