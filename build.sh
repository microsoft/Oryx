#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && pwd )

declare -r buildSolutionScript="$REPO_DIR/build/build-solution.sh"
declare -r testBuildImagesScript="$REPO_DIR/build/test-buildimages.sh"
declare -r testRuntimeImagesScript="$REPO_DIR/build/test-runtimeimages.sh"
declare -r testScriptGeneratorScript="$REPO_DIR/build/test-scriptgenerator.sh"
declare -r testIntegrationScript="$REPO_DIR/build/test-integration.sh"
declare -r testNodeStartupCommandGenerator="$REPO_DIR/build/test-nodestartupgenerator.sh"

# Build soulution to verify there are basic errors (like compilation)
# before doing the expensive docker image builds
$buildSolutionScript "$@"

# Test script generator tests
$testScriptGeneratorScript "$@"

# Build build & runtime images and run their tests
$testBuildImagesScript "$@"
$testRuntimeImagesScript "$@"

# Test on AKS
$testIntegrationScript "$@"

# Test the Node.JS startup script generator
$testNodeStartupCommandGenerator "$@"