#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

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

testProjectName="Oryx.Integration"
cd "$TESTS_SRC_DIR/$testProjectName.Tests"

# These three images are used in Buildpacks-related integration tests
docker pull "heroku/buildpacks:18"
docker pull "heroku/pack:18"
docker pull "mcr.microsoft.com/oryx/pack-stack-base"

dotnet test \
    $testCaseFilter \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR/testResults/$testProjectName$integrationTestPlatform.Tests.xml" \
    -c $BUILD_CONFIGURATION