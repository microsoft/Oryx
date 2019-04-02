#!/bin/bash

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

wget -nv "https://github.com/buildpack/pack/releases/download/v0.0.9/pack-0.0.9-$packPlatform.tar.gz"
tar -xvf "pack-0.0.9-$packPlatform.tar.gz"
# `/tmp/pack` is now available for use

./pack add-stack com.microsoft.oryx.stack -b 7ba7a9b720d1 -r 7ba7a9b720d1
./pack set-default-stack com.microsoft.oryx.stack
