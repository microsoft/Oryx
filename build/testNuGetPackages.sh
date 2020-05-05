#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

RunTest() {
    local testProjectName="$1"

    cd "$TESTS_SRC_DIR/NuGetPackagesTests/$testProjectName"
    
    # Run restore with no cache so that the latest version of packages are always used
    dotnet restore --no-cache

    dotnet test \
        --test-adapter-path:. \
        --logger:"xunit;LogFilePath=$ARTIFACTS_DIR\testResults\\$testProjectName.xml" \
        -c $BUILD_CONFIGURATION
}

echo
RunTest "BuildScriptGeneratorNuGetPackage.Tests"

echo
RunTest "BuildScriptGeneratorCliNuGetPackage.Tests"