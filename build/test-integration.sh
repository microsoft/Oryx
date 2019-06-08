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
    function getValueFromKeyVault() {
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
    echo Retreiving values from keyvault...
    export SQLSERVER_DATABASE_HOST=$(getValueFromKeyVault "SQLSERVER-DATABASE-HOST")
    export SQLSERVER_DATABASE_NAME=$(getValueFromKeyVault "SQLSERVER-DATABASE-NAME")
    export SQLSERVER_DATABASE_USERNAME=$(getValueFromKeyVault "SQLSERVER-DATABASE-USERNAME")
    export SQLSERVER_DATABASE_PASSWORD=$(getValueFromKeyVault "SQLSERVER-DATABASE-PASSWORD")
fi

echo

if [ -n "$1" ]; then
    testCaseFilter="--filter $1"
	echo "Running integration tests with filter '$testCaseFilter'..."
else
	echo "Running all integration tests..."
fi

echo

testProjectName="Oryx.Integration.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"

# These two images are used in Buildpacks-related integration tests
docker pull heroku/buildpacks:18
docker pull heroku/pack:18

dotnet test \
    $testCaseFilter \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR/testResults/$testProjectName.xml" \
    -c $BUILD_CONFIGURATION
