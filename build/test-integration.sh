#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

if [ -z "$STORAGEACCOUNTKEY" ]; then # If STORAGEACCOUNTKEY is unset or empty
	export STORAGEACCOUNTKEY=`az.cmd storage account keys list -n oryxautomation | grep key1 | awk '{print $NF}'`
fi

# Load all variables
source $REPO_DIR/build/__variables.sh

echo
echo Pulling docker images required for running tests against databases ...
docker pull mysql/mysql-server:5.7
docker pull postgres
docker pull microsoft/mssql-server-linux:2017-CU12

echo
echo "Building and running tests..."
testProjectName="Oryx.Integration.Tests"
cd "$TESTS_SRC_DIR/$testProjectName"
# Run non-AKS tests temporarily
dotnet test --filter E2E!=AKS --test-adapter-path:. --logger:"xunit;LogFilePath=artifacts\testResults\\$testProjectName.xml" -c $BUILD_CONFIGURATION