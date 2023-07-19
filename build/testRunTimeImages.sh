#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r buildRuntimeImagesScript="$REPO_DIR/build/buildRunTimeImages.sh"
declare -r testProjectName="Oryx.RuntimeImage.Tests"

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__sdkStorageConstants.sh

if [ -z "$ORYX_TEST_SDK_STORAGE_URL" ]; then
    echo
    echo "Setting environment variable 'ORYX_TEST_SDK_STORAGE_URL' to default value '$PRIVATE_STAGING_SDK_STORAGE_BASE_URL' for runtime image tests."
    export ORYX_TEST_SDK_STORAGE_URL="$PRIVATE_STAGING_SDK_STORAGE_BASE_URL"
fi

if [ "$1" = "skipBuildingImages" ]
then
    echo
    echo "Skipping building runtime images as argument '$1' was passed..."
else
    echo
    echo "Invoking script '$buildRuntimeImagesScript'..."
    $buildRuntimeImagesScript -s $ORYX_TEST_SDK_STORAGE_URL "$@"
fi

if [ -n "$2" ]
then
    echo
    echo "Setting environment variable 'ORYX_TEST_IMAGE_BASE' to provided value '$2'."
    export ORYX_TEST_IMAGE_BASE="$2"
fi

if [ -n "$3" ]
then
    echo
    echo "Setting environment variable 'ORYX_TEST_TAG_SUFFIX' to provided value '$3'."
    export ORYX_TEST_TAG_SUFFIX="-$3"
fi

if [ -n "$4" ]; then
    testCaseFilter="--filter $4"
    echo "Running runtime image tests with filter '$testCaseFilter'..."
else
    echo "Running all runtime image tests..."
fi

echo
echo "Building and running tests..."
cd "$TESTS_SRC_DIR/$testProjectName"

artifactsDir="$REPO_DIR/artifacts"
mkdir -p "$artifactsDir"

# Create a directory to capture any debug logs that MSBuild generates
msbuildDebugLogsDir="$artifactsDir/msbuildDebugLogs"
mkdir -p "$msbuildDebugLogsDir"
export MSBUILDDEBUGPATH="$msbuildDebugLogsDir"
# Enable automatic creation of crash dump when a .NET Core process crashes (ex: TestHost)
export COMPlus_DbgEnableMiniDump="1"
export COMPlus_DbgMiniDumpName="$ARTIFACTS_DIR/$testProjectName-dump.%d"

diagnosticFileLocation="$artifactsDir/$testProjectName-log.txt"
dotnet test \
    --blame \
    --diag "$diagnosticFileLocation" \
    $testCaseFilter \
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