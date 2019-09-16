#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
#
# This script builds some base images that are needed for the build image:
# - Python binaries
# - PHP binaries
# - Yarn package cache
#

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

IMAGE_DIR_TO_BUILD=$1
IMAGE_TAG="${BUILD_NUMBER:-latest}"
BUILD_IMAGE_PREFIX="$REPO_DIR/images/build"
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$IMAGE_DIR_TO_BUILD-buildimage-bases.txt"

# Clean artifacts
mkdir -p `dirname $ARTIFACTS_FILE`
> $ARTIFACTS_FILE

case $IMAGE_DIR_TO_BUILD in
	'python')
		echo "Building Python base images"
		echo

		docker build -f $BUILD_IMAGE_PREFIX/python/prereqs/Dockerfile -t "python-build-prereqs" $REPO_DIR

		declare -r PYTHON_IMAGE_PREFIX="$ACR_DEV_NAME/public/oryx/python-build"

		docker build -f $BUILD_IMAGE_PREFIX/python/2.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-2.7:$IMAGE_TAG" $REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-2.7:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.6/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.6:$IMAGE_TAG" $REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.6:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.7:$IMAGE_TAG" $REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.7:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.8/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.8:$IMAGE_TAG" $REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.8:$IMAGE_TAG" >> $ARTIFACTS_FILE
		;;
	'php')
		echo "Building PHP base images"
		echo

		docker build -f $BUILD_IMAGE_PREFIX/php/prereqs/Dockerfile -t "php-build-prereqs" $REPO_DIR

		declare -r PHP_IMAGE_PREFIX="$ACR_DEV_NAME/public/oryx/php-build"

		docker build -f $BUILD_IMAGE_PREFIX/php/5.6/Dockerfile -t "$PHP_IMAGE_PREFIX-5.6:$IMAGE_TAG" $REPO_DIR
		echo "$PHP_IMAGE_PREFIX-5.6:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.0/Dockerfile -t "$PHP_IMAGE_PREFIX-7.0:$IMAGE_TAG" $REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.0:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.2/Dockerfile -t "$PHP_IMAGE_PREFIX-7.2:$IMAGE_TAG" $REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.2:$IMAGE_TAG" >> $ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.3/Dockerfile -t "$PHP_IMAGE_PREFIX-7.3:$IMAGE_TAG" $REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.3:$IMAGE_TAG" >> $ARTIFACTS_FILE
		;;            
	'yarn-cache')
		echo "Building Yarn package cache base image"
		echo

		YARN_CACHE_IMAGE_BASE="$ACR_DEV_NAME/public/oryx/build-yarn-cache"
		YARN_CACHE_IMAGE_NAME=$YARN_CACHE_IMAGE_BASE:$IMAGE_TAG

		docker build $BUILD_IMAGE_PREFIX/yarn-cache -t $YARN_CACHE_IMAGE_NAME
		echo $YARN_CACHE_IMAGE_NAME >> $ARTIFACTS_FILE
		;;
	*) echo "Unknown image directory";;
esac

echo
echo "List of images built (from '$ARTIFACTS_FILE'):"
cat $ARTIFACTS_FILE
echo