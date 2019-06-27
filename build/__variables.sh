#!/bin/bash
set -e

declare -r __REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# VSTS environment variables
declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r BUILD_CONFIGURATION="${BUILDCONFIGURATION:-Debug}"

declare -r BUILD_IMAGES_BUILD_CONTEXT_DIR="$__REPO_DIR/"
declare -r BUILD_IMAGES_DOCKERFILE="$__REPO_DIR/images/build/Dockerfile"
declare -r PACK_IMAGE_DOCKERFILE="$__REPO_DIR/images/pack-builder/pack-runner.Dockerfile"
declare -r PACK_STACK_BASE_IMAGE_DOCKERFILE="$__REPO_DIR/images/pack-builder/stack-base.Dockerfile"
declare -r ORYXTESTS_BUILDIMAGE_DOCKERFILE="$__REPO_DIR/tests/images/build/Dockerfile"
declare -r RUNTIME_IMAGES_SRC_DIR="$__REPO_DIR/images/runtime"
declare -r SOURCES_SRC_DIR="$__REPO_DIR/src"
declare -r TESTS_SRC_DIR="$__REPO_DIR/tests"

declare -r ARTIFACTS_DIR="$__REPO_DIR/artifacts"
declare -r BUILD_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/build-images.txt"
declare -r BASE_IMAGES_ARTIFACTS_FILE_PREFIX="$ARTIFACTS_DIR/images"
declare -r RUNTIME_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/runtime-images.txt"
declare -r ACR_BUILD_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/build-images-acr.txt"
declare -r ACR_RUNTIME_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/runtime-images-acr.txt"

declare -r DOCKER_DEV_REPO_BASE='oryxdevms'
declare -r DOCKER_BUILD_IMAGES_REPO="$DOCKER_DEV_REPO_BASE/build"
declare -r PACK_IMAGE_NAME='pack'
declare -r DOCKER_PACK_IMAGE_REPO="$DOCKER_DEV_REPO_BASE/$PACK_IMAGE_NAME"
declare -r PACK_STACK_BASE_IMAGE_NAME="pack-stack-base"
declare -r DOCKER_PACK_STACK_BASE_IMAGE_REPO="$DOCKER_DEV_REPO_BASE/$PACK_STACK_BASE_IMAGE_NAME"
declare -r PACK_BUILDER_IMAGE_NAME="pack-builder"
declare -r DOCKER_PACK_BUILDER_IMAGE_REPO="$DOCKER_DEV_REPO_BASE/$PACK_BUILDER_IMAGE_NAME"
declare -r ORYXTESTS_BUILDIMAGE_REPO="oryxtests/build"
declare -r DOCKER_RUNTIME_IMAGES_REPO=$DOCKER_DEV_REPO_BASE
declare -r ACR_DEV_NAME="oryxdevmcr.azurecr.io"
declare -r ACR_PUBLIC_PREFIX="$ACR_DEV_NAME/public/oryx"
declare -r ACR_BUILD_IMAGES_REPO="$ACR_DEV_NAME/public/oryx/build"
declare -r ACR_RUNTIME_IMAGES_REPO="$ACR_PUBLIC_PREFIX"
declare -r ACR_PACK_BUILDER_IMAGE_REPO="$ACR_PUBLIC_PREFIX/$PACK_BUILDER_IMAGE_NAME"
declare -r ACR_PACK_IMAGE_REPO="$ACR_PUBLIC_PREFIX/$PACK_IMAGE_NAME"
declare -r ACR_PACK_STACK_BASE_IMAGE_REPO="$ACR_PUBLIC_PREFIX/$PACK_STACK_BASE_IMAGE_NAME"

# Flag to add information to images through labels (example: build number, commit sha)
declare -r EMBED_BUILDCONTEXT_IN_IMAGES="${EMBEDBUILDCONTEXTINIMAGES:-false}"
declare -r GIT_COMMIT=$(git rev-parse HEAD)

declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-false}"

# Filter to find any uncategorized integration tests
declare -r MISSING_CATEGORY_FILTER="category!=node&category!=python&category!=php&category!=dotnetcore&category!=db"
declare -r INTEGRATION_TEST_PROJECT="category!=node&category!=python&category!=php&category!=dotnetcore&category!=db"
