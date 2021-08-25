#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

PLATFORM_TO_BUILD=$1
DEBIAN_FLAVOR_TO_BUILD=$2
platformsDir="$REPO_DIR/platforms"

# TODO: find a better place for chmod
chmod +x $platformsDir/golang/getGolangSdk.sh

case $PLATFORM_TO_BUILD in
	'dotnet')
		"$platformsDir/dotnet/getDotNetSdk.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'python')
		"$platformsDir/python/buildPython.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'php')
		"$platformsDir/php/buildPhp.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'nodejs')
		"$platformsDir/nodejs/getNode.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'golang')
		"$platformsDir/golang/getGolangSdk.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'ruby')
		"$platformsDir/ruby/buildRuby.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'java')
		"$platformsDir/java/getJavaSdk.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;     
	'maven')
		"$platformsDir/java/maven/getMaven.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;            
	*) echo "Unknown image directory";;
esac