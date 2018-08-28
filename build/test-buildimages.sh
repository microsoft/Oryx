#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildBuildImagesScript="$REPO_DIR/build/build-buildimages.sh"

# Load all variables
source $REPO_DIR/build/__variables.sh

echo
echo "Invoking script '$buildBuildImagesScript'..."
$buildBuildImagesScript "$@"

echo
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/Oryx.BuildImage.Tests"
dotnet test -c $BUILD_CONFIGURATION