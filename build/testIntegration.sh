#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__sdkStorageConstants.sh

if [ -z "$ORYX_TEST_SDK_STORAGE_URL" ]; then
    echo
    echo "Setting environment variable 'ORYX_TEST_SDK_STORAGE_URL' to default value '$PRIVATE_STAGING_SDK_STORAGE_BASE_URL' for integration tests."
    export ORYX_TEST_SDK_STORAGE_URL="$PRIVATE_STAGING_SDK_STORAGE_BASE_URL"
fi

# When this script is run in CI agent these environment variables are already set
if [ -z "$SQLSERVER_DATABASE_HOST" ]; then
    function getSecretFromKeyVault() {
        local secretName="$1"
        result=`az.cmd keyvault secret show \
                                        --name "$secretName" \
                                        --vault-name "oryx" \
                                        | grep value \
                                        | awk '{print $NF}' \
                                        | tr -d '"'`
        echo $result
    }

    echo
    echo Retrieving secrets from Azure Key Vault...
    export SQLSERVER_DATABASE_HOST=$(getSecretFromKeyVault "SQLSERVER-DATABASE-HOST")
    export SQLSERVER_DATABASE_NAME=$(getSecretFromKeyVault "SQLSERVER-DATABASE-NAME")
    export SQLSERVER_DATABASE_USERNAME=$(getSecretFromKeyVault "SQLSERVER-DATABASE-USERNAME")
    export SQLSERVER_DATABASE_PASSWORD=$(getSecretFromKeyVault "SQLSERVER-DATABASE-PASSWORD")
fi

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
    -c $BUILD_CONFIGURATION

# --blame flag generates an xml file which it drops under the project directory.
# Copy that file to artifacts directory too
if [ -d "TestResults" ]; then
    resultsDir="$ARTIFACTS_DIR/$testProjectName.TestResults"
    mkdir -p "$resultsDir"
    cp -rf TestResults/. "$resultsDir/"
fi