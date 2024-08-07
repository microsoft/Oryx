#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__sdkStorageConstants.sh

echo

# This is needed because when we are running tests in multiple agent machines
# this variable will be used to name the testresults file and that way we can
# avoid overwriting test results file.
integrationTestPlatform=".default"

if [ -n "$1" ]; then
    testCaseFilter="--filter $1"
    if [ -n "$AGENT_BUILD" ]; then
        # Extract platform name for which the integration tests are running
        # for example, for node it will be ".node", for php ".php" etc.
        integrationTestPlatform="."$(echo $1 | cut -d'=' -f 2)
    fi
    echo "Running integration tests for '$integrationTestPlatform' with filter '$testCaseFilter'..."
else
    echo "Running all integration tests..."
fi

echo

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

echo

testProjectName="Oryx.Integration"
cd "$TESTS_SRC_DIR/$testProjectName.Tests"
artifactsDir="$REPO_DIR/artifacts"
mkdir -p "$artifactsDir"
diagnosticFileLocation="$artifactsDir/$testProjectName.Tests$integrationTestPlatform-log.txt"

# Create a directory to capture any debug logs that MSBuild generates
msbuildDebugLogsDir="$artifactsDir/msbuildDebugLogs"
mkdir -p "$msbuildDebugLogsDir"
export MSBUILDDEBUGPATH="$msbuildDebugLogsDir"
# Enable automatic creation of crash dump when a .NET Core process crashes (ex: TestHost)
export COMPlus_DbgEnableMiniDump="1"
export COMPlus_DbgMiniDumpName="$ARTIFACTS_DIR/$testProjectName.Tests-dump.%d"

dotnet test \
    --blame \
    --diag "$diagnosticFileLocation" \
    $testCaseFilter \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR/testResults/$testProjectName$integrationTestPlatform.Tests.xml" \
    --verbosity detailed \
    -c $BUILD_CONFIGURATION

# --blame flag generates an xml file which it drops under the project directory.
# Copy that file to artifacts directory too
if [ -d "TestResults" ]; then
    resultsDir="$ARTIFACTS_DIR/$testProjectName.TestResults"
    mkdir -p "$resultsDir"
    cp -rf TestResults/. "$resultsDir/"
fi