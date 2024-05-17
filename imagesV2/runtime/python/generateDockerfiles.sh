#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r PYTHON_VERSIONS_PATH=$DIR/../../../build/__pythonVersions.sh
declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
# Python major version, e.g. '2', '3'
declare -r PYTHON_MAJOR_VERSION_PLACEHOLDER="%PYTHON_MAJOR_VERSION%"
# Python version as we usually refer to, e.g. '2.7', '3.6'
declare -r PYTHON_VERSION_PLACEHOLDER="%PYTHON_VERSION%"
# Python full version, including patch, e.g. '3.7.3'
declare -r PYTHON_FULL_VERSION_PLACEHOLDER="%PYTHON_FULL_VERSION%"
declare -r ORYX_IMAGE_TAG_PLACEHOLDER="%IMAGE_TAG%"
declare -r RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER="%RUNTIME_BASE_IMAGE_TAG%"
declare -r ORYX_PYTHON_IMAGE_BASE_PLACEHOLDER="%BASE_TAG%"

source "$PYTHON_VERSIONS_PATH"

# Please make sure that any changes to debian flavors supported here are also reflected in build/constants.yaml
declare -r PYTHON_BOOKWORM_VERSION_ARRAY=($PYTHON38_VERSION $PYTHON39_VERSION $PYTHON310_VERSION $PYTHON311_VERSION $PYTHON312_VERSION)
declare -r PYTHON_BULLSEYE_VERSION_ARRAY=($PYTHON37_VERSION $PYTHON38_VERSION $PYTHON39_VERSION $PYTHON310_VERSION $PYTHON311_VERSION $PYTHON312_VERSION)
declare -r PYTHON_BUSTER_VERSION_ARRAY=($PYTHON37_VERSION $PYTHON38_VERSION $PYTHON39_VERSION $PYTHON310_VERSION)
ImageDebianFlavor="$1"
echo "python baseimage type: $ImageDebianFlavor"

VERSIONS_DIRECTORY=()

if [ "$ImageDebianFlavor" == "bookworm" ];then
    VERSIONS_DIRECTORY=("${PYTHON_BOOKWORM_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "bullseye" ];then
    VERSIONS_DIRECTORY=("${PYTHON_BULLSEYE_VERSION_ARRAY[@]}")
elif [ "$ImageDebianFlavor" == "buster" ];then
    VERSIONS_DIRECTORY=("${PYTHON_BUSTER_VERSION_ARRAY[@]}")
fi


for VERSION_DIRECTORY in "${VERSIONS_DIRECTORY[@]}"
do
    #PYTHON_VERSION=${!PYTHON_VERSION_VAR_NAME}
    IFS='.' read -ra SPLIT_VERSION <<< "$VERSION_DIRECTORY"
    #VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    # IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"
    MAJOR_MINOR_VERSION="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    mkdir -p "$DIR/$MAJOR_MINOR_VERSION/"
    TARGET_DOCKERFILE="$DIR/$MAJOR_MINOR_VERSION/$ImageDebianFlavor.Dockerfile"
    cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

    ORYX_PYTHON_IMAGE_BASE_TAG="oryx-run-base-$ImageDebianFlavor"

    # Replace placeholders
    sed -i "s|$PYTHON_VERSION_PLACEHOLDER|$MAJOR_MINOR_VERSION|g" "$TARGET_DOCKERFILE"
    sed -i "s|$PYTHON_FULL_VERSION_PLACEHOLDER|$VERSION_DIRECTORY|g" "$TARGET_DOCKERFILE"
    sed -i "s|$PYTHON_MAJOR_VERSION_PLACEHOLDER|${SPLIT_VERSION[0]}|g" "$TARGET_DOCKERFILE"
    sed -i "s|$ORYX_IMAGE_TAG_PLACEHOLDER|$PYTHON_BASE_TAG|g" "$TARGET_DOCKERFILE"
    sed -i "s|$ORYX_PYTHON_IMAGE_BASE_PLACEHOLDER|$ORYX_PYTHON_IMAGE_BASE_TAG|g" "$TARGET_DOCKERFILE"

    RUNTIME_BASE_IMAGE_TAG="python-$MAJOR_MINOR_VERSION-debian-$ImageDebianFlavor-$PYTHON_RUNTIME_BASE_TAG"
    sed -i "s|$RUNTIME_BASE_IMAGE_TAG_PLACEHOLDER|$RUNTIME_BASE_IMAGE_TAG|g" "$TARGET_DOCKERFILE"

done < <(compgen -A variable | grep 'PYTHON[0-9]\{2,\}_VERSION')