#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

echo

if [ -n "$1" ]; then
    testCaseFilter="--filter $1"
	echo "Running integration tests with '$testCaseFilter'..."
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
