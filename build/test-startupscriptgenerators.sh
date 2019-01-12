#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r GEN_DIR="$REPO_DIR/src/startupscriptgenerator/"
# When volume mounting a directory from the host machine, we host it as a readonly folder because any modifications by a
# container in that folder would be owned by 'root' user(as containers run as 'root' by default). Since CI build agents
# run as non-root cleaning these files would be a problem. So we copy the mounted directory in the container
# to a different directory within the container itself and run tests on it.
declare -r GEN_DIR_CONTAINER_RO="/src"
declare -r GEN_DIR_CONTAINER="/go/src/startupscriptgenerator"
declare -r MODULE_TO_TEST="startupscriptgenerator/..."
declare -r CONTAINER_NAME="oryxtests_$RANDOM"

echo "Running tests in golang docker image..."
docker run -v $GEN_DIR:$GEN_DIR_CONTAINER_RO:ro --name $CONTAINER_NAME golang:1.11-stretch bash -c \
	"cp -rf $GEN_DIR_CONTAINER_RO $GEN_DIR_CONTAINER && cd $GEN_DIR_CONTAINER && ./prepare-go-env.sh && \
	go test $MODULE_TO_TEST -v"