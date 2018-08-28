#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && pwd )

declare -r buildSolutionScript="$REPO_DIR/build/build-solution.sh"
declare -r testBuildImagesScript="$REPO_DIR/build/test-buildimages.sh"
declare -r testRuntimeImagesScript="$REPO_DIR/build/test-runtimeimages.sh"

# Build soulution to verify there are basic errors (like compilation)
# before doing the expensive docker image builds
$buildSolutionScript "$@"

# Build build & runtime images and run their tests
$testBuildImagesScript "$@"
$testRuntimeImagesScript "$@"

# Copy artifacts related to 'src' folder to the artifacts directory
# TODO


