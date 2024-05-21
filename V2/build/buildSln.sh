#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

solutionFileName="$1"

if [ -z "$solutionFileName" ]; then
    solutionFileName="Oryx.sln"
fi

# Load all variables
source $REPO_DIR/build/__variables.sh

echo
echo "Building solution '$solutionFileName'..."
echo
cd $REPO_DIR
dotnet build "$solutionFileName" -c $BUILD_CONFIGURATION