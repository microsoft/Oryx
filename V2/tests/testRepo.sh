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
NODE_VERSION=10.14
PYTHON_VERSION=3.7
DOTNETCORE_VERSION=2.2

IMAGE_HOST=docker.io
IMAGE_USER=oryxprod

BUILD_IMAGE="${IMAGE_HOST}/${IMAGE_USER}/build:${ORYX_VERSION}"
RUN_IMAGE_NODEJS="${IMAGE_HOST}/${IMAGE_USER}/node-${NODE_VERSION}:${ORYX_VERSION}"
RUN_IMAGE_PYTHON="${IMAGE_HOST}/${IMAGE_USER}/python-${PYTHON_VERSION}:${ORYX_VERSION}"
RUN_IMAGE_DOTNETCORE="${IMAGE_HOST}/${IMAGE_USER}/dotnetcore-${DOTNETCORE_VERSION}:${ORYX_VERSION}"

LOGFILE_PATH="./test-repo.log"

function test-repo() {
    local repo_path=${1:-"$(pwd)"}
    local host_port=${2:-"8080"}
    local container_port=${3:-"8080"}
    local runtime=${4:-"nodejs"}
    local start_script=${5:-""}

    DOCKER_FLAGS=''
    if [[ -e "${repo_path}/.env" ]]; then
        DOCKER_FLAGS+="--env-file ${repo_path}/.env"
    fi

    # build
    docker pull ${BUILD_IMAGE}
    docker run --interactive --tty \
        --volume "$repo_path":/repo \
        ${DOCKER_FLAGS} \
        "$BUILD_IMAGE" \
        sh -c "oryx build --log-file ${LOGFILE_PATH} /repo"

    # run
    case $runtime in
        nodejs)
            RUN_IMAGE="${RUN_IMAGE_NODEJS}"
            ;;
        python)
            RUN_IMAGE="${RUN_IMAGE_PYTHON}"
            ;;
        dotnetcore)
            RUN_IMAGE="${RUN_IMAGE_DOTNETCORE}"
            ;;
    esac
    
    TEST_CONTAINER_NAME=oryx-test-repo
    cid=$(docker container ls \
      --all --filter "name=${TEST_CONTAINER_NAME}" --quiet)
    if [[ -n "$cid" ]]; then docker stop $cid; docker rm $cid; fi

    docker pull ${RUN_IMAGE}
    docker run --detach \
        --name ${TEST_CONTAINER_NAME} \
        --volume $(pwd):/app \
        --publish ${host_port}:${container_port} \
        --env PORT=${container_port} \
        ${DOCKER_FLAGS} \
        "$RUN_IMAGE" \
        sh -c "cd /app && oryx && /app/run.sh" 
}

test-repo $@