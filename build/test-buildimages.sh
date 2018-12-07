#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildBuildImagesScript="$REPO_DIR/build/build-buildimages.sh"
declare -r testProjectName="Oryx.BuildImage.Tests"

# Load all variables
source $REPO_DIR/build/__variables.sh

if [ "$1" = "skipBuildingImages" ]
then
    echo
    echo "Skipping building build images as argument '$1' was passed..."
else
    echo
    echo "Invoking script '$buildBuildImagesScript'..."
    $buildBuildImagesScript "$@"
fi

echo
echo Pulling docker images required for running tests against databases ...
docker pull mysql/mysql-server:5.7
docker pull postgres
docker pull microsoft/mssql-server-linux:2017-CU12

echo
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/$testProjectName"
dotnet test --test-adapter-path:. --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION