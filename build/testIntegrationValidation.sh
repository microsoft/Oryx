#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

# Success or Failure we should let the tests to run.
testResult=$(dotnet test --filter "$MISSING_CATEGORY_FILTER" "$TESTS_SRC_DIR/$INTEGRATION_TEST_PROJECT/$INTEGRATION_TEST_PROJECT.csproj")

if [[ $testResult == *"No test matches the given testcase filter"* ]]; then 
    echo
    echo "All integration tests have categories: No missing category tests found..."  
else 
    echo "Some Uncategorized integration tests found ..."
    echo "$testResult"
    exit 1
fi