#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

echo
echo "Current list of running processes:"
ps aux | less
echo

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildBuildImagesScript="$REPO_DIR/build/buildBuildImages.sh"
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
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/$testProjectName"

artifactsDir="$REPO_DIR/artifacts"
mkdir -p "$artifactsDir"
diagnosticFileLocation="$artifactsDir/$testProjectName-log.txt"

# Create a directory to capture any debug logs that MSBuild generates
msbuildDebugLogsDir="$artifactsDir/msbuildDebugLogs"
mkdir -p "$msbuildDebugLogsDir"
export MSBUILDDEBUGPATH="$msbuildDebugLogsDir"
export MSBUILDDISABLENODEREUSE=1

dotnet test \
    --blame \
    --diag "$diagnosticFileLocation" \
    --verbosity diag \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" \
    -c $BUILD_CONFIGURATION

# --blame flag generates an xml file which it drops under the project directory.
# Copy that file to artifacts directory too
if [ -d "TestResults" ]; then
    resultsDir="$ARTIFACTS_DIR/$testProjectName.TestResults"
    mkdir -p "$resultsDir"
    cp -rf TestResults/. "$resultsDir/"
fi