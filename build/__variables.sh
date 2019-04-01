#!/bin/bash
set -e

declare -r __REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# VSTS environment variables
declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r BUILD_CONFIGURATION="${BUILDCONFIGURATION:-Debug}"

declare -r BUILD_IMAGES_BUILD_CONTEXT_DIR="$__REPO_DIR/"
declare -r BUILD_IMAGE_DOCKERFILE="$__REPO_DIR/images/build/Dockerfile"
declare -r ORYXTESTS_BUILDIMAGE_DOCKERFILE="$__REPO_DIR/tests/images/build/Dockerfile"
declare -r RUNTIME_IMAGES_SRC_DIR="$__REPO_DIR/images/runtime"
declare -r SOURCES_SRC_DIR="$__REPO_DIR/src"
declare -r TESTS_SRC_DIR="$__REPO_DIR/tests"

declare -r ARTIFACTS_DIR="$__REPO_DIR/artifacts"
declare -r BUILD_IMAGE_BASES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/build-image-bases.txt"
declare -r DOCKERHUB_BUILD_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/dockerhub-build-images.txt"
declare -r DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/dockerhub-runtime-images.txt"
declare -r ACR_BUILD_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/acr-build-images.txt"
declare -r ACR_RUNTIME_IMAGES_ARTIFACTS_FILE="$ARTIFACTS_DIR/images/acr-runtime-images.txt"

declare -r LOCAL_BUILD_IMAGE_REPO="oryx/build"
declare -r LOCAL_RUNTIME_IMAGES_REPO_PREFIX="oryx"
declare -r ORYXTESTS_BUILDIMAGE_REPO="oryxtests/build"
declare -r DOCKERHUB_BUILD_IMAGE_REPO="oryxdevms/build"
declare -r DOCKERHUB_RUNTIME_IMAGES_REPO_PREFIX="oryxdevms"
declare -r ACR_DEV_NAME="oryxdevmcr.azurecr.io"
declare -r ACR_BUILD_IMAGE_REPO="$ACR_DEV_NAME/public/oryx/build"
declare -r ACR_RUNTIME_IMAGES_REPO_PREFIX="$ACR_DEV_NAME/public/oryx"

# Flag to add information to images through labels (example: build number, commit sha)
declare -r EMBED_BUILDCONTEXT_IN_IMAGES="${EMBEDBUILDCONTEXTINIMAGES:-false}"
declare -r GIT_COMMIT=$(git rev-parse HEAD)

declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-false}"
