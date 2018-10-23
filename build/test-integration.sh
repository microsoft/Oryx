#!/bin/bash
set -e

declare -r testProjectName="Oryx.Integration.Tests"

# Load all variables
source ./build/__variables.sh

echo "Building and running tests..."
cd "./tests/$testProjectName"
dotnet test --test-adapter-path:. --logger:"xunit;LogFilePath=artifacts\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION