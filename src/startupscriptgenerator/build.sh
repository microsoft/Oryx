#!/bin/sh

# This script is intended to be copied with the startup command generator to the container to
# build it, thus avoiding having repeat commands in each Dockerfile. It assumes that the source
# is properly mapped in $GOPATH. The language and target binary as passed as a positional arguments.

set -e

if [[ "$1" == "" || "$2" == "" ]]; then
    echo "Usage: build <language> <target output>"
    echo "Language should match the directory name of the language-specific implementation."
    echo "Target output is the path to the Linux binary to be produced."
    exit 1
fi

LANGUAGE=$1
TARGET_OUTPUT=$2
DIR=$(dirname "$0")

echo "Trying to find the language..."
cd "$DIR/$LANGUAGE"
go build -o "$TARGET_OUTPUT" .