#!/bin/bash

declare -r PACK_VERSION='0.1.0'

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

wget -nv "https://github.com/buildpack/pack/releases/download/v$PACK_VERSION/pack-v$PACK_VERSION-$packPlatform.tgz"
tar -xvf "pack-v$PACK_VERSION-$packPlatform.tgz"
# `./pack` is now available for use
