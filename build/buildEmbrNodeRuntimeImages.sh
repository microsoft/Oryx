#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/embrconstants.yml"
readonly DEFAULT_CONSTANTS_FILE="$REPO_DIR/images/constants.yml"
readonly DOCKERFILE="$REPO_DIR/images/runtime/node/embr/template.Dockerfile"
readonly ARTIFACTS_DIR="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$REPO_DIR/artifacts}"
readonly IMAGE_LIST="$ARTIFACTS_DIR/images/embr-node-runtime-images.resolute.txt"
readonly NODE_VERSIONS="${EMBR_NODE_VERSIONS:-22 24 26}"
readonly IMAGE_REPOSITORY="${EMBR_NODE_IMAGE_REPOSITORY:-embr-node-runtime}"
readonly BASE_IMAGE="docker.io/library/embr_oryx_run_base_resolute"

yaml_value() {
    local file="$1"
    local key="$2"
    sed -n "s/^  ${key}:[[:space:]]*//p" "$file"
}

embr_yaml_value() {
    local key="$1"
    yaml_value "$CONSTANTS_FILE" "$key"
}

default_yaml_value() {
    local key="$1"
    yaml_value "$DEFAULT_CONSTANTS_FILE" "$key"
}

mkdir -p "$(dirname "$IMAGE_LIST")"
: > "$IMAGE_LIST"

yarn_version="$(embr_yaml_value "yarnVersion")"
npm_version="$(default_yaml_value "NPM_VERSION")"
pm2_version="$(default_yaml_value "PM2_VERSION")"

if [[ -z "$yarn_version" || -z "$npm_version" || -z "$pm2_version" ]]; then
    echo "Missing Node dependency metadata in $CONSTANTS_FILE or $DEFAULT_CONSTANTS_FILE." >&2
    exit 1
fi

docker build \
    --file "$REPO_DIR/images/runtime/commonbase/Dockerfile" \
    --tag "$BASE_IMAGE" \
    --build-arg "OS_FLAVOR=resolute" \
    --build-arg "OS_TYPE=ubuntu" \
    "$REPO_DIR"

for major_version in $NODE_VERSIONS; do
    full_version="$(embr_yaml_value "node${major_version}Version")"

    if [[ -z "$full_version" ]]; then
        echo "Missing version for Node $major_version in $CONSTANTS_FILE." >&2
        exit 1
    fi

    image="$IMAGE_REPOSITORY:$major_version-resolute"
    docker build \
        --file "$DOCKERFILE" \
        --tag "$image" \
        --build-arg "BASE_IMAGE=$BASE_IMAGE" \
        --build-arg "NODE_FULL_VERSION=$full_version" \
        --build-arg "NPM_VERSION=$npm_version" \
        --build-arg "PM2_VERSION=$pm2_version" \
        --build-arg "YARN_VERSION=$yarn_version" \
        --build-arg "AI_CONNECTION_STRING=${AI_CONNECTION_STRING:-}" \
        --build-arg "BUILD_NUMBER=${BUILD_BUILDNUMBER:-local}" \
        --build-arg "GIT_COMMIT=${BUILD_SOURCEVERSION:-unspecified}" \
        --build-arg "RELEASE_TAG_NAME=${RELEASE_TAG_NAME:-local}" \
        "$REPO_DIR"
    echo "$image" >> "$IMAGE_LIST"
done

echo "Built local Embr Node runtime images:"
cat "$IMAGE_LIST"
