#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/embrconstants.yml"
readonly DOCKERFILE="$REPO_DIR/images/runtime/node/embr/template.Dockerfile"
readonly ARTIFACTS_DIR="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$REPO_DIR/artifacts}"
readonly IMAGE_LIST="$ARTIFACTS_DIR/images/embr-node-runtime-images.resolute.txt"
readonly NODE_VERSIONS="${EMBR_NODE_VERSIONS:-22 24}"
readonly IMAGE_REPOSITORY="${EMBR_NODE_IMAGE_REPOSITORY:-embr-node-runtime}"

yaml_value() {
    local key="$1"
    sed -n "s/^  ${key}:[[:space:]]*//p" "$CONSTANTS_FILE"
}

mkdir -p "$(dirname "$IMAGE_LIST")"
: > "$IMAGE_LIST"

yarn_version="$(yaml_value "yarnVersion")"
yarn_url="$(yaml_value "yarnSourceUrl")"
yarn_sha256="$(yaml_value "yarnSource_SHA256")"

if [[ -z "$yarn_version" || -z "$yarn_url" || -z "$yarn_sha256" ]]; then
    echo "Missing Yarn source metadata in $CONSTANTS_FILE." >&2
    exit 1
fi

for major_version in $NODE_VERSIONS; do
    full_version="$(yaml_value "node${major_version}Version")"
    sha256="$(yaml_value "node${major_version}_SHA256")"

    if [[ -z "$full_version" || -z "$sha256" ]]; then
        echo "Missing version or SHA256 for Node $major_version in $CONSTANTS_FILE." >&2
        exit 1
    fi

    minor_version="${full_version%.*}"
    image="$IMAGE_REPOSITORY:$major_version-resolute"
    docker build \
        --file "$DOCKERFILE" \
        --tag "$image" \
        --build-arg "NODE_FULL_VERSION=$full_version" \
        --build-arg "NODE_VERSION=$minor_version" \
        --build-arg "NODE_MAJOR_VERSION=$major_version" \
        --build-arg "NODE_SHA256=$sha256" \
        --build-arg "YARN_VERSION=$yarn_version" \
        --build-arg "YARN_URL=$yarn_url" \
        --build-arg "YARN_SHA256=$yarn_sha256" \
        --build-arg "BUILD_NUMBER=${BUILD_BUILDNUMBER:-local}" \
        --build-arg "GIT_COMMIT=${BUILD_SOURCEVERSION:-unspecified}" \
        --build-arg "RELEASE_TAG_NAME=${RELEASE_TAG_NAME:-local}" \
        "$REPO_DIR"
    echo "$image" >> "$IMAGE_LIST"
done

echo "Built local Embr Node runtime images:"
cat "$IMAGE_LIST"
