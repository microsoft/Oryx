#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/embrconstants.yml"
readonly DOCKERFILE="$REPO_DIR/images/runtime/python/embr/template.Dockerfile"
readonly ACR_NAME="${ACR_NAME:-oryxdevmcr.azurecr.io}"
readonly ACR_REPOSITORY="${ACR_REPOSITORY:-public/oryx/python}"
readonly RELEASE_TAG_NAME="${RELEASE_TAG_NAME:-local}"
readonly BUILD_DEFINITIONNAME="${BUILD_DEFINITIONNAME:-local}"
readonly ARTIFACTS_DIR="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$REPO_DIR/artifacts}"
readonly IMAGE_LIST="$ARTIFACTS_DIR/images/embr-python-runtime-images-acr.resolute.txt"
readonly PYTHON_VERSIONS="${EMBR_PYTHON_VERSIONS:-3.13 3.14 3.15}"

yaml_value() {
    local key="$1"
    sed -n "s/^  ${key}:[[:space:]]*//p" "$CONSTANTS_FILE"
}

mkdir -p "$(dirname "$IMAGE_LIST")"
: > "$IMAGE_LIST"

for minor_version in $PYTHON_VERSIONS; do
    compact_version="${minor_version//./}"
    full_version="$(yaml_value "python${compact_version}Version")"
    sha256="$(yaml_value "python${compact_version}_SHA256")"

    if [[ -z "$full_version" || -z "$sha256" ]]; then
        echo "Missing version or SHA256 for Python $minor_version in $CONSTANTS_FILE." >&2
        exit 1
    fi

    image="$ACR_NAME/$ACR_REPOSITORY:embr-$minor_version-ubuntu-resolute-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
    docker build \
        --file "$DOCKERFILE" \
        --tag "$image" \
        --build-arg "PYTHON_FULL_VERSION=$full_version" \
        --build-arg "PYTHON_VERSION=$minor_version" \
        --build-arg "PYTHON_SHA256=$sha256" \
        --build-arg "BUILD_NUMBER=${BUILD_BUILDNUMBER:-local}" \
        --build-arg "GIT_COMMIT=${BUILD_SOURCEVERSION:-unspecified}" \
        --build-arg "RELEASE_TAG_NAME=$RELEASE_TAG_NAME" \
        "$REPO_DIR"
    echo "$image" >> "$IMAGE_LIST"
done

echo "Built Embr Python runtime images:"
cat "$IMAGE_LIST"
