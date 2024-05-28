#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

PLATFORM_TO_BUILD=$1
DEBIAN_FLAVOR_TO_BUILD=$2
SDK_STORAGE_ACCOUNT_URL=$3
platformsDir="$REPO_DIR/platforms"

case $PLATFORM_TO_BUILD in
	'dotnet')
		chmod +x "$platformsDir/dotnet/getDotNetSdk.sh"
		"$platformsDir/dotnet/getDotNetSdk.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'python')
		chmod +x "$platformsDir/python/buildPython.sh"
		"$platformsDir/python/buildPython.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'php')
		chmod +x "$platformsDir/php/buildPhp.sh"
		"$platformsDir/php/buildPhp.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'nodejs')
		chmod +x "$platformsDir/nodejs/getNode.sh"
		"$platformsDir/nodejs/getNode.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'golang')
		chmod +x "$platformsDir/golang/getGolangSdk.sh"
		"$platformsDir/golang/getGolangSdk.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'ruby')
		chmod +x "$platformsDir/ruby/buildRuby.sh"
		"$platformsDir/ruby/buildRuby.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;
	'java')
		chmod +x "$platformsDir/java/getJavaSdk.sh"
		"$platformsDir/java/getJavaSdk.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;     
	'maven')
		chmod +x "$platformsDir/java/maven/getMaven.sh"
		"$platformsDir/java/maven/getMaven.sh" $DEBIAN_FLAVOR_TO_BUILD $SDK_STORAGE_ACCOUNT_URL
		;;            
	*) echo "Unknown image directory";;
esac
