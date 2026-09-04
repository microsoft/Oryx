#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly SOURCE_FILE="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/embr-python-runtime-images-acr.resolute.txt"
readonly OUTPUT_FILE="$BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images/oryxprodmcr-runtime-images-mcr.txt"
readonly PROD_REPOSITORY="oryxprodmcr.azurecr.io/public/oryx/python"

if [[ ! -s "$SOURCE_FILE" ]]; then
    echo "Embr Python runtime image list not found: $SOURCE_FILE" >&2
    exit 1
fi

touch "$OUTPUT_FILE"

while IFS= read -r source_image; do
    source_image="${source_image%$'\r'}"
    [[ -n "$source_image" ]] || continue
    minor_version="$(sed -n 's/.*:embr-\([0-9]\+\.[0-9]\+\)-ubuntu-resolute-.*/\1/p' <<< "$source_image")"
    if [[ -z "$minor_version" ]]; then
        echo "Unexpected Embr Python image tag: $source_image" >&2
        exit 1
    fi

    immutable_image="$PROD_REPOSITORY:embr-$minor_version-ubuntu-resolute-$RELEASE_TAG_NAME"
    docker pull "$source_image"
    docker tag "$source_image" "$immutable_image"
    echo "$immutable_image" >> "$OUTPUT_FILE"

    if [[ "$BUILD_SOURCEBRANCHNAME" == "main" ]]; then
        moving_image="$PROD_REPOSITORY:embr-$minor_version-ubuntu-resolute"
        docker tag "$source_image" "$moving_image"
        echo "$moving_image" >> "$OUTPUT_FILE"
    fi
done < "$SOURCE_FILE"

echo "Prepared Embr Python release tags:"
cat "$OUTPUT_FILE"
