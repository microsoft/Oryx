#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

PLATFORM_TO_BUILD=$1
platformsDir="$REPO_DIR/platforms"

case $PLATFORM_TO_BUILD in
	'dotnet')
		"$platformsDir/dotnet/getDotNetSdk.sh"
		;;
	'python')
		"$platformsDir/python/buildPython.sh"
		;;
	'php')
		"$platformsDir/php/buildPhp.sh"
		;;
	'node')
		"$platformsDir/node/getNode.sh"
		;;            
	*) echo "Unknown image directory";;
esac
