#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# This script is intended to be copied with the startup command generator to the container to
# build it, thus avoiding having repeat commands in each Dockerfile. It assumes that the source
# is properly mapped in $GOPATH. The platform and target binary as passed as a positional arguments.

set -e

if [ "$1" = "" ] || [ "$2" = "" ]; then
    echo "Usage: build <platform> <target output>"
    echo "Platform should match the directory name of the platform-specific implementation."
    echo "Target output is the path to the Linux binary to be produced."
    exit 1
fi

declare -r WORKSPACE_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

PLATFORM=$1
PLATFORM_DIR="$WORKSPACE_DIR/src/$PLATFORM"
TARGET_OUTPUT=$2
export GOPATH="$WORKSPACE_DIR"

if [ ! -d "$PLATFORM_DIR" ]; then
    echo "Invalid platform name '$PLATFORM'. Could not find directory '$PLATFORM_DIR'."
    exit 1
fi

echo "Building the package for platform '$PLATFORM'..."
cd "$PLATFORM_DIR"

cp -r $WORKSPACE_DIR/src/common /usr/local/go/src/common
cp -r $WORKSPACE_DIR/src/common/consts /usr/local/go/src/common/consts
if [[ ! -f "go.mod" ]] ; then
    go mod init
fi
go mod tidy

go build \
    -ldflags "-X common.BuildNumber=$BUILD_NUMBER -X common.Commit=$GIT_COMMIT -X common.ReleaseTagName=$RELEASE_TAG_NAME" \
    -v -o "$TARGET_OUTPUT" .
