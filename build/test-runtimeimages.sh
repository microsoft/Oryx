#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildRuntimeImagesScript="$REPO_DIR/build/build-runtimeimages.sh"

# Load all variables
source $REPO_DIR/build/__variables.sh

echo
echo "Invoking script '$buildRuntimeImagesScript'..."
$buildRuntimeImagesScript "$@"

echo
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/Oryx.RuntimeImage.Tests"
dotnet test -c $BUILD_CONFIGURATION