#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

cd $REPO_DIR

# Get comma separated list of all constants files
constantsFiles=$(find "$REPO_DIR/build" -type f -name '*constants.yaml' -o -name '*constants.yml')
constantsFiles=$(echo $constantsFiles | tr ' ' ,)

dotnet run \
	--no-launch-profile \
	--project $REPO_DIR/build/tools/SharedCodeGenerator/SharedCodeGenerator.csproj \
	-- \
	"$constantsFiles" \
	"$REPO_DIR"
