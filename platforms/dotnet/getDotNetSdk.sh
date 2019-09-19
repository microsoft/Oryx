#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

dotNetPlatformDir="$REPO_DIR/platforms/dotnet"

getDotNetCoreSdk() {
	local version="$1"
	local sha="$2"
	local downloadUrl="$3"
	local targetDir="$volumeHostDir/dotnet"
	mkdir -p "$targetDir"

	if blobExists dotnet dotnet-$version.tar.gz; then
		echo ".NET Core version '$version' already present in blob storage. Skipping it..."
		echo
	else
		echo "Downloading .NET Core SDK version '$version'..."
		echo

		if [ -z "$downloadUrl" ]; then
			# Use default download url file
			downloadUrl="https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$version/dotnet-sdk-$version-linux-x64.tar.gz"
		fi

		# Follow the format used by all platforms
		downloadedFile="dotnet-$version.tar.gz"
		mkdir -p /tmp/oryx-dotnetInstall
		cd /tmp/oryx-dotnetInstall
		rm -f "$downloadedFile"
		curl -SL $downloadUrl --output "$downloadedFile"
		echo "Verifying archive hash..."
		echo "$sha $downloadedFile" | sha512sum -c -
		cp -f "$downloadedFile" "$targetDir"
		rm -rf /tmp/oryx-dotnetInstall
	fi
	
	echo "$version" >> "$targetDir/versions.txt"
}

echo "Getting .NET Core Sdks..."
echo
buildPlatform "$dotNetPlatformDir/versions.txt" getDotNetCoreSdk