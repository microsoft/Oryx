#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

declare -r PACK_VERSION='0.26.0'

if [[ "$OSTYPE" == "linux-gnu" ]]; then
	packPlatform='linux';
elif [[ "$OSTYPE" == "darwin"* ]]; then
	packPlatform='macos';
elif [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "msys" ]]; then
	echo 'ERROR: `pack create-builder` is not implemented on Windows.'
	exit 1
else
	echo 'ERROR: Could not detect compatible pack binary platform.'
	exit 1
fi

packTar="pack-v$PACK_VERSION-$packPlatform.tgz"
if [ ! -f "$packTar" ]; then
	wget -nv "https://github.com/buildpack/pack/releases/download/v$PACK_VERSION/$packTar"
fi
tar -xvf "$packTar"
# `./pack` is now available for use
