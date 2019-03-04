#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

cd $REPO_DIR
dotnet run --no-launch-profile --project $REPO_DIR/build/tools/SharedCodeGenerator/SharedCodeGenerator.csproj -- $REPO_DIR/build/build-constants.yaml $REPO_DIR
