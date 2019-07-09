#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-true}"

# The following code snippet is a hack to workaround the following issue:
# Our CI agents run under a non-root account, but docker containers run under the 'root' account.
# Since a non-root account cannot delete files created by a 'root' account, we try to run a
# docker container and run the clean up command from there using the volume mounted directory trick.
# 
# This is required for end-to-end tests to work, where a host's directory is volume mounted to a build
# image and the same host directory is volume mounted to a runtime image too.
#
# Even though Docker has a way to run a container as a non-root (for example, in our case the CI agent account),
# this causes problems in build image where we try to install packages and stuff and we run into permission problems.
# Since that is too intrusive and error prone, this hack allows us to not worry about those permission issues.

mountedDirs="/tmp/OryxTestsMountedDirs"
echo
echo "Mounted Directory: "$mountedDirs
echo

if [ -d "$mountedDirs" ]; then
    echo
    echo "Cleaning up files created by test containers ..."
    docker run -v $mountedDirs:/tempDirs oryxdevmcr.azurecr.io/public/oryx/build /bin/bash -c "rm -rf /tempDirs/* && ls /tempDirs"
fi