#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-true}"

mountedDirs="/tmp/OryxTestsMountedDirs"
docker volume ls

echo
echo "Mounted Directory: "
echo

if [ -d "$mountedDirs" ]; then
	echo
	echo "Cleaning up files created by tests' docker containers ..."
	rm -rf $mountedDirs && ls /tmp
fi