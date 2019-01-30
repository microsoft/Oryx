#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Description:
#   Run from root of a repo to build and run as a containerized app.
#   By default publish host port 8080 mapped to container port 8080.
#   If available, `build.env` specifies build-time env vars.
#   $5 specifies a user-specified start script.
# 
# Example:
#   # build and run a Python app listening on host port 88
#   ./test-repo.sh ./app 88 8088 python

ORYX_VERSION=latest
NODE_VERSION=10.12
PYTHON_VERSION=3.7

BUILD_IMAGE="mcr.microsoft.com/oryx/build:${ORYX_VERSION}"
RUN_IMAGE_NODEJS="mcr.microsoft.com/oryx/node-${NODE_VERSION}:${ORYX_VERSION}"
RUN_IMAGE_PYTHON="mcr.microsoft.com/oryx/python-${PYTHON_VERSION}:${ORYX_VERSION}"

function test-repo() {
    local repo_path=${1:-"$(pwd)"}
    local host_port=${2:-"8080"}
    local container_port=${3:-"8080"}
    local runtime=${4:-"nodejs"}
    local start_script=${5:-""}

    rm_buildenv=false
    rm_runenv=false
    if [[ ! -e build.env ]]; then
        touch build.env
        rm_buildenv=true
    fi
    if [[ ! -e run.env ]]; then
        touch run.env
        rm_runenv=true
    fi

    # build
    docker pull ${BUILD_IMAGE}
    docker run --interactive --tty \
        --volume "$repo_path":/repo \
        --env-file build.env \
        "$BUILD_IMAGE" \
        sh -c 'oryx build /repo'

    # run
    case $runtime in
        nodejs)
            RUN_IMAGE="$RUN_IMAGE_NODEJS"
            ;;
        python)
            RUN_IMAGE="$RUN_IMAGE_PYTHON"
            ;;
    esac

    TEST_CONTAINER_NAME=oryx-test-repo
    cid=$(docker container ls \
      --all --filter "name=${TEST_CONTAINER_NAME}" --quiet)
    if [[ -n "$cid" ]]; then docker stop $cid; docker rm $cid; fi

    docker pull ${RUN_IMAGE}
    docker run --interactive --tty \
        --name ${TEST_CONTAINER_NAME} \
        --volume $(pwd):/app \
        --publish ${host_port}:${container_port} \
        --env PORT=${container_port} \
        --env-file run.env \
        "$RUN_IMAGE" \
        sh -c "cd /app && oryx && /app/run.sh" 

    if [[ "$rm_buildenv" == "true" ]]; then rm build.env; fi
    if [[ "$rm_runenv" == "true" ]]; then rm run.env; fi
}

test-repo $@
