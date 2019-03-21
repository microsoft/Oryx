#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
#
# This script can be used to build the base platforms locally, before building the build image.
# In Azure DevOps, this is taken care of by the pipelines defined in vsts/pipelines/buildimage-platforms.
#

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r ACR_NAME='oryxdevmcr.azurecr.io'

# Build Python
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "$ACR_NAME/python-build-prereqs" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "$ACR_NAME/python27-build" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "$ACR_NAME/python35-build" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "$ACR_NAME/python36-build" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "$ACR_NAME/python37-build" $REPO_DIR
