#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-true}"

mountedDirs="/tmp/OryxTestsMountedDirs"

echo
echo "Mounted Directory: "$mountedDirs
echo

if [ -d "$mountedDirs" ]; then
    echo
    echo "Cleaning up files created by test containers ..."
    docker run -v $mountedDirs:/tempDirs oryxdevms/build /bin/bash -c "rm -rf /tempDirs/* && ls /tempDirs"
fi