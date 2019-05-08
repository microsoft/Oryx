#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

if [ -n "$1" ]
then
    testCaseFilter="--filter $1"
fi

echo
echo "Running integration tests with filter '$testCaseFilter'..."
echo
testProjectName="Oryx.Integration.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"

true || dotnet test \
    $testCaseFilter \
    --test-adapter-path:. \
    --logger:"xunit;LogFilePath=$ARTIFACTS_DIR/testResults/$testProjectName.xml" \
    -c $BUILD_CONFIGURATION
