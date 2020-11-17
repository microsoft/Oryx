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

case $PLATFORM_TO_BUILD in
	'dotnet')
		"$platformsDir/dotnet/getDotNetSdk.sh"
		;;
	'python')
		"$platformsDir/python/buildPython.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'php')
		"$platformsDir/php/buildPhp.sh" $DEBIAN_FLAVOR_TO_BUILD
		;;
	'nodejs')
		"$platformsDir/nodejs/getNode.sh"
		;;
	'ruby')
		"$platformsDir/ruby/buildRuby.sh"
		;;
	'java')
		"$platformsDir/java/getJavaSdk.sh"
		;;     
	'maven')
		"$platformsDir/java/maven/getMaven.sh"
		;;            
	*) echo "Unknown image directory";;
esac