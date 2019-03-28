#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

cd $REPO_DIR/src/BuildScriptGeneratorCli
dotnet publish BuildScriptGeneratorCli.csproj -c Release -o $1
