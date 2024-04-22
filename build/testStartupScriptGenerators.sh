#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

if [[ "$OSTYPE" == "linux-gnu" ]] || [[ "$OSTYPE" == "darwin"* ]]; then
	declare -r GEN_DIR="$REPO_DIR/src/startupscriptgenerator/src"
else
	# When running this script on Windows, for example on dev machines
	declare -r GEN_DIR=$(cmd.exe /C  "echo %CD%\..\src\startupscriptgenerator\src")
fi

# When volume mounting a directory from the host machine, we host it as a readonly folder because any modifications by a
# container in that folder would be owned by 'root' user(as containers run as 'root' by default). Since CI build agents
# run as non-root cleaning these files would be a problem. So we copy the mounted directory in the container
# to a different directory within the container itself and run tests on it.
declare -r GEN_DIR_CONTAINER_RO="/startupscriptgenerator"
declare -r GEN_DIR_CONTAINER="/go/src"
declare -r MODULE_TO_TEST="..."
declare -r CONTAINER_NAME="oryxtests_$RANDOM"

echo "Running tests in golang docker image..."
docker run -v $GEN_DIR:$GEN_DIR_CONTAINER_RO:ro --name $CONTAINER_NAME mcr.microsoft.com/oss/go/microsoft/golang:1.22 bash -c \
	"cp -rf $GEN_DIR_CONTAINER_RO/* $GEN_DIR_CONTAINER && \
	cd $GEN_DIR_CONTAINER && \
	chmod u+x restorePackages.sh && \
	./restorePackages.sh && \
	echo && \
	echo Running tests... && \
	chmod u+x testPackages.sh && \
	./testPackages.sh"