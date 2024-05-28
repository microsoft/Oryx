#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

# A hint to Detector.csproj file to generate the nuget package in a specific way so that
# test projects can restore it
export CREATE_PACKAGE_FOR_TESTS="true"

echo
echo "Building and running tests..."

testProjectName="Detector.Tests"
echo
echo "Running tests in project '$testProjectName'..."
echo
cd "$TESTS_SRC_DIR/$testProjectName"
mkdir -p "$ARTIFACTS_DIR"
dotnet test \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" \
    -c $BUILD_CONFIGURATION \
    | sed 's/^/    /'

testProjectName="Detector.NuGetPackage.Tests"
export NUGET_PACKAGES="$REPO_DIR/tests/$testProjectName/.package-cache"

echo
echo "Deleting existing Detector packages from cache..."
shopt -s nocaseglob
rm -rf "$NUGET_PACKAGES/Microsoft.Oryx.Detector"

echo
echo "Running tests in project '$testProjectName'..."
echo
cd "$TESTS_SRC_DIR/$testProjectName"
dotnet test \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" \
    -c $BUILD_CONFIGURATION \
    | sed 's/^/    /'

# --blame flag generates an xml file which it drops under the project directory.
# Copy that file to artifacts directory too
if [ -d "TestResults" ]; then
    resultsDir="$ARTIFACTS_DIR/$testProjectName.TestResults"
    mkdir -p "$resultsDir"
    cp -rf TestResults/. "$resultsDir/"
fi