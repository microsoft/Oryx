#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variablesMCR.sh

echo
echo "Building and running tests..."
testProjectName="BuildScriptGenerator.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"
dotnet test --test-adapter-path:. --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION

testProjectName="BuildScriptGeneratorCli.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"
dotnet test --test-adapter-path:. --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION