#!/bin/bash

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r VERSIONS_FILE="$DIR/nodeVersions.txt"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r NODE_VERSION_PLACEHOLDER="%NODE_BASE_IMAGE%"

while IFS= read -r NODE_VERSION || [[ -n $NODE_VERSION ]]
do
    IFS='.' read -ra SPLIT_VERSION <<< "$NODE_VERSION"
    VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
    echo "Processing version $NODE_VERSION in directory $VERSION_DIRECTORY"
    mkdir -p "$DIR/$VERSION_DIRECTORY/"
    TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
    cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"
    sed -i "s/$NODE_VERSION_PLACEHOLDER/$NODE_VERSION/g" "$TARGET_DOCKERFILE"

done < "$VERSIONS_FILE"