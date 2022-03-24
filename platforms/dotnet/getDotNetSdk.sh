#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

dotNetPlatformDir="$REPO_DIR/platforms/dotnet"
targetDir="$volumeHostDir/dotnet"
debianFlavor="$1"
mkdir -p "$targetDir"

getDotNetCoreSdk() {
	local sdkVersion="$1"
	local sha="$2"
	local downloadUrl="$3"
	local downloadedFile=""

	if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			downloadedFile=dotnet-$sdkVersion.tar.gz
	else
			downloadedFile=dotnet-$debianFlavor-$sdkVersion.tar.gz
	fi

	if shouldBuildSdk dotnet $downloadedFile || shouldOverwriteSdk || shouldOverwritePlatformSdk dotnet; then
		echo "Downloading .NET Core SDK version '$sdkVersion'..."
		echo

		if [ -z "$downloadUrl" ]; then
			# Use default download url file
			downloadUrl="https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$sdkVersion/dotnet-sdk-$sdkVersion-linux-x64.tar.gz"
		fi

		tempDir="/tmp/oryx-dotnetInstall"
		mkdir -p $tempDir
		cd $tempDir
		rm -f "$downloadedFile"
		curl -SL $downloadUrl --output "$downloadedFile"
		echo "Verifying archive hash..."
		echo "$sha $downloadedFile" | sha512sum -c -
		
		# Find the runtime version
		tar -xzf $downloadedFile -C .
		runtimeVersionDir=$(find "$tempDir/shared/Microsoft.NETCore.App" -mindepth 1 -maxdepth 1 -type d)
		runtimeVersion=$(basename $runtimeVersionDir)
		cp -f "$downloadedFile" "$targetDir"
		rm -rf $tempDir

		echo "Runtime_version=$runtimeVersion" >> "$targetDir/dotnet-$sdkVersion-metadata.txt"
		echo "Version=$sdkVersion" >> "$targetDir/dotnet-$sdkVersion-metadata.txt"
	fi
}

echo
echo "Getting .NET Core Sdks..."
echo
buildPlatform "$dotNetPlatformDir/versionsToBuild.txt" getDotNetCoreSdk

cp "$dotNetPlatformDir/defaultVersion.txt" $targetDir
