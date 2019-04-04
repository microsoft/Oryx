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

source __variables.sh

IMAGE_TAG="${BUILD_NUMBER:-latest}"
BUILD_IMAGE_PREFIX="$__REPO_DIR/images/build"

# Clean artifacts
mkdir -p `dirname $BUILD_IMAGE_BASES_ARTIFACTS_FILE`
> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

case $1 in
	'python')
		# Build Python
		docker build -f $BUILD_IMAGE_PREFIX/python/prereqs/Dockerfile -t "python-build-prereqs" $__REPO_DIR

		declare -r PYTHON_IMAGE_PREFIX="$ACR_DEV_NAME/public/oryx/python-build"

		docker build -f $BUILD_IMAGE_PREFIX/python/2.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-2.7:$IMAGE_TAG" $__REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-2.7:$IMAGE_TAG" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.5/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.5:$IMAGE_TAG" $__REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.5:$IMAGE_TAG" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.6/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.6:$IMAGE_TAG" $__REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.6:$IMAGE_TAG" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/python/3.7/Dockerfile -t "$PYTHON_IMAGE_PREFIX-3.7:$IMAGE_TAG" $__REPO_DIR
		echo "$PYTHON_IMAGE_PREFIX-3.7:$IMAGE_TAG" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE
		;;
	'php')
		# Build PHP
		docker build -f $BUILD_IMAGE_PREFIX/php/prereqs/Dockerfile -t "php-build-prereqs" $__REPO_DIR

		declare -r PHP_IMAGE_PREFIX="$ACR_DEV_NAME/public/oryx/php-build"

		docker build -f $BUILD_IMAGE_PREFIX/php/5.6/Dockerfile -t "$PHP_IMAGE_PREFIX-5.6:$BUILD_NUMBER" $__REPO_DIR
		echo "$PHP_IMAGE_PREFIX-5.6:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.0/Dockerfile -t "$PHP_IMAGE_PREFIX-7.0:$BUILD_NUMBER" $__REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.0:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.2/Dockerfile -t "$PHP_IMAGE_PREFIX-7.2:$BUILD_NUMBER" $__REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.2:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE

		docker build -f $BUILD_IMAGE_PREFIX/php/7.3/Dockerfile -t "$PHP_IMAGE_PREFIX-7.3:$BUILD_NUMBER" $__REPO_DIR
		echo "$PHP_IMAGE_PREFIX-7.3:$BUILD_NUMBER" >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE
		;;            
	'yarn-cache')
		# Build Yarn cache
		YARN_CACHE_IMAGE_BASE="$ACR_DEV_NAME/public/oryx/build-yarn-cache"
		YARN_CACHE_IMAGE_NAME=$YARN_CACHE_IMAGE_BASE:$IMAGE_TAG

		docker build $BUILD_IMAGE_PREFIX/yarn-cache -t $YARN_CACHE_IMAGE_NAME
		echo $YARN_CACHE_IMAGE_NAME >> $BUILD_IMAGE_BASES_ARTIFACTS_FILE
		;;
	*) echo "Unknown image directory";;
esac

echo
echo "List of images built (from '$BUILD_IMAGE_BASES_ARTIFACTS_FILE'):"
cat $BUILD_IMAGE_BASES_ARTIFACTS_FILE
echo
