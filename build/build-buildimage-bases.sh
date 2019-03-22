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

source $REPO_DIR/build/__variables.sh

# Clean artifacts
touch $BUILD_IMAGE_BASES_ARTIFACTS_FILE
> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

# Build Python
docker build -f $REPO_DIR/images/build/python/prereqs/Dockerfile -t "python-build-prereqs" $REPO_DIR

declare -r PYTHON_IMAGE_PREFIX="$ACR_DEV_NAME/public/oryx/python-build"

docker build -f $REPO_DIR/images/build/python/2.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-2.7:$BUILD_NUMBER" $REPO_DIR
echo "$PYTHON_IMAGE_PREFIX-2.7:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

docker build -f $REPO_DIR/images/build/python/3.5/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.5:$BUILD_NUMBER" $REPO_DIR
echo "$PYTHON_IMAGE_PREFIX-3.5:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

docker build -f $REPO_DIR/images/build/python/3.6/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.6:$BUILD_NUMBER" $REPO_DIR
echo "$PYTHON_IMAGE_PREFIX-3.6:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

docker build -f $REPO_DIR/images/build/python/3.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.7:$BUILD_NUMBER" $REPO_DIR
echo "$PYTHON_IMAGE_PREFIX-3.7:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE


echo
echo "List of images built (from '$RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $BUILD_IMAGE_BASES_ARTIFACTS_FILE
echo
