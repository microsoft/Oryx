#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r GEN_DIR="$REPO_DIR/src/startupscriptgenerator/"
declare -r GEN_DIR_CONTAINER="/go/src/startupscriptgenerator"
declare -r MODULE_TO_TEST="startupscriptgenerator/..."
declare -r CONTAINER_NAME="oryxtests_$RANDOM"

echo "Running tests in golang docker image..."
docker run -v $GEN_DIR:$GEN_DIR_CONTAINER:ro --name $CONTAINER_NAME -w $GEN_DIR_CONTAINER golang:1.11-stretch bash -c ". prepare-go-env.sh; go test $MODULE_TO_TEST -v"