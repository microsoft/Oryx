#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildRuntimeImagesScript="$REPO_DIR/build/build-runtimeimagesMCR.sh"
declare -r testProjectName="Oryx.RuntimeImage.Tests"

# Load all variables
source $REPO_DIR/build/__variablesMCR.sh

if [ "$1" = "skipBuildingImages" ]
then
    echo
    echo "Skipping building runtime images as argument '$1' was passed..."
else
    echo
    echo "Invoking script '$buildRuntimeImagesScript'..."
    $buildRuntimeImagesScript "$@"
fi

echo
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/$testProjectName"
dotnet test --test-adapter-path:. --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION