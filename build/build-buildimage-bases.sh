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

# Build Python
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "python-build-prereqs" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/2.7/Dockerfile -t "mcr.microsoft.com/oryx/python-build-2.7" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/3.5/Dockerfile -t "mcr.microsoft.com/oryx/python-build-3.5" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/3.6/Dockerfile -t "mcr.microsoft.com/oryx/python-build-3.6" $REPO_DIR
docker build -f $REPO_DIR/images/build/python/3.7/Dockerfile -t "mcr.microsoft.com/oryx/python-build-3.7" $REPO_DIR
