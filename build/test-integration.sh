#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

if [ -z "$STORAGEACCOUNTKEY" ]; then # If STORAGEACCOUNTKEY is unset or empty
	export STORAGEACCOUNTKEY=`az.cmd storage account keys list -n oryxautomation | grep key1 | awk '{print $NF}'`
fi

echo
echo "Building and running tests..."
testProjectName="Oryx.Integration.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"
# Run non-AKS tests temporarily
dotnet test --filter E2E!=AKS --test-adapter-path:. --logger:"xunit;LogFilePath=$ARTIFACTS_DIR/testResults/$testProjectName.xml" -c $BUILD_CONFIGURATION
