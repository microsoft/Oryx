#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__pythonVersions.sh

pythonPlatformDir="$REPO_DIR/platforms/python"
targetDir="$volumeHostDir/python"
debianFlavor=$1
mkdir -p "$targetDir"

builtPythonPrereqs=false
buildPythonPrereqsImage() {
    debianType=$debianFlavor
	# stretch is out of support for python, but we still need to build stretch based
	# binaries because of static sites, they need to move to buster based jamstack image
	# before we can remove this hack
    if [ "$debianFlavor" == "stretch" ]; then
        debianType="focal-scm"
    fi

	if ! $builtPythonPrereqs; then
		echo "Building Python pre-requisites image..."
		echo
		docker build  \
			   --build-arg DEBIAN_FLAVOR=$debianFlavor \
			   --build-arg DEBIAN_HACK_FLAVOR=$debianType \
			   -f "$pythonPlatformDir/prereqs/Dockerfile"  \
			   -t "oryxdevmcr.azurecr.io/private/oryx/python-build-prereqs" $REPO_DIR
		builtPythonPrereqs=true
	fi
}

buildPython() {
	local version="$1"
	local gpgKey="$2"
	local dockerFile="$3"
	local imageName="oryx/python"
	local pythonSdkFileName=""

	if [ "$debianFlavor" == "stretch" ]; then
			# Use default python sdk file name
			pythonSdkFileName=python-$version.tar.gz
	else
			pythonSdkFileName=python-$debianFlavor-$version.tar.gz
	fi

	if shouldBuildSdk python $pythonSdkFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk python; then
		if ! $builtPythonPrereqs; then
			buildPythonPrereqsImage
		fi
		
		echo "Building Python version '$version' in a docker image..."
		echo

        echo "dockerfile is : $pythonPlatformDir/$dockerFile"
		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$pythonPlatformDir/Dockerfile"
		else
			dockerFile="$pythonPlatformDir/$dockerFile"
		fi
		
		cat $dockerFile

		docker build \
			-f "$dockerFile" \
			--build-arg VERSION_TO_BUILD=$version \
			--build-arg GPG_KEYS=$gpgKey \
			--build-arg PIP_VERSION=$PIP_VERSION \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "Version=$version" >> "$targetDir/python-$version-metadata.txt"
	fi
}

echo "Building Python..."
echo
buildPlatform "$pythonPlatformDir/versionsToBuild.txt" buildPython

# Write the default version
cp "$pythonPlatformDir/defaultVersion.txt" $targetDir