#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__integrationTestCategory.sh

integrationTestProject="Oryx.Integration.Tests"

missingCategoryFilter="category!=$FILTER_NODE& \
						 category!=$FILTER_PYTHON& \
						 category!=$FILTER_PHP& \
						 category!=$FILTER_DOTNETCORE& \
						 category!=$FILTER_DB"

dotnetTestArg="$missingCategoryFilter $TESTS_SRC_DIR/$integrationTestProject/$integrationTestProject.csproj"

# Success or Failure we should let the tests to run.
testResult=$(dotnet test --filter $dotnetTestArg)

if [[ $testResult == *"No test matches the given testcase filter"* ]]; then 
    echo
    echo "All integration tests have categories: No missing category tests found..."  
else 
    echo "Some Uncategorized integration tests found ..."
    echo "$testResult"
    exit 1
fi