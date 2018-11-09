#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r GEN_DIR=$REPO_DIR/src/StartupScriptGenerator-Node
declare -r APP_DIR_CONTAINER="/app"
declare -r CONTAINER_NAME="oryxtests_$RANDOM"

echo "Running tests from golang docker image..."
docker run -v $GEN_DIR:$APP_DIR_CONTAINER:ro --name $CONTAINER_NAME -w $APP_DIR_CONTAINER golang:1.11-alpine go test . -v