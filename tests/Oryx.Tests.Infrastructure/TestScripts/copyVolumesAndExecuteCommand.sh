#!/bin/bash

if [ ! $# -gt 2 ]
then
	echo "Error: Usage: <MountedVolumeRootDir> <WritableVolumeRootDir> <command> [arguments]"
	exit 1
fi

MountedVolumeRootDir=$1
WritableVolumeRootDir=$2

if [ ! -d "$MountedVolumeRootDir" ]
then
	echo "Error: Directory '$MountedVolumeRootDir' does not exist."
	exit 1
fi

echo
echo
echo "Creating directory '$WritableVolumeRootDir'..."
mkdir -p "$WritableVolumeRootDir"

echo
echo "Copying files from '$MountedVolumeRootDir' to '$WritableVolumeRootDir'..."
cp -r "$MountedVolumeRootDir/." "$WritableVolumeRootDir"

echo
echo "Running command '${@:3}'..."
eval ${@:3}