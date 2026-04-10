#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Regenerates shared constant files (C#, Shell, Go) from build/constants.yaml
# using the SharedCodeGenerator tool.

set -e

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Generating constants from ${REPO_DIR}/build/constants.yaml ..."
dotnet run --project "${REPO_DIR}/build/tools/SharedCodeGenerator/SharedCodeGenerator.csproj" \
    "${REPO_DIR}/build/constants.yaml" \
    "${REPO_DIR}"

echo "Constants generated successfully."
