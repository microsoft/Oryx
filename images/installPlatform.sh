#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source $CURRENT_DIR/__common.sh

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
  case "$1" in
    -d|--dir)
      targetDir=$2
      shift 2
      ;;
    -l|--links)
      createLinks=$2
      shift 2
      ;;
    --) # end argument parsing
      shift
      break
      ;;
    -*|--*=) # unsupported flags
      echo "Error: Unsupported flag $1" >&2
      exit 1
      ;;
    *) # preserve positional arguments
      PARAMS="$PARAMS $1"
      shift
      ;;
  esac
done
# set positional arguments in their proper place
eval set -- "$PARAMS"

PLATFORM_NAME="$1"
VERSION="$2"

debianFlavor=$DEBIAN_FLAVOR
fileName="$PLATFORM_NAME-$VERSION.tar.gz"

if [ -z "$debianFlavor" ] || [ "$debianFlavor" == "stretch" ]; then
  # Use default sdk file name
	fileName="$PLATFORM_NAME-$VERSION.tar.gz"
else
  fileName="$PLATFORM_NAME-$debianFlavor-$VERSION.tar.gz"
fi

platformDir="/opt/$PLATFORM_NAME"

if [ -z "$targetDir" ]; then
    targetDir="$platformDir/$VERSION"
fi

START_TIME=$SECONDS
downloadFileAndVerifyChecksum $PLATFORM_NAME $VERSION $fileName
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Downloaded and verified checksum in $ELAPSED_TIME sec(s)."

echo "Extracting..."
START_TIME=$SECONDS
mkdir -p "$targetDir"
tar -xzf $fileName -C "$targetDir"
rm -f "$fileName"
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Extracted contents in $ELAPSED_TIME sec(s)."

if [ "$PLATFORM_NAME" == "python" ]
then
   [ -d "/opt/python/$VERSION" ] && echo /opt/python/$VERSION/lib >> /etc/ld.so.conf.d/python.conf
   ldconfig
fi

if [ "$createLinks" != "false" ]; then
    # Create a link : major.minor => major.minor.patch
    cd "$platformDir"
    IFS='.' read -ra VERSION_PARTS <<< "$VERSION"
    MAJOR_MINOR="${VERSION_PARTS[0]}.${VERSION_PARTS[1]}"
    echo
    echo "Created link from $MAJOR_MINOR to $VERSION"
    ln -sfn "$VERSION" "$MAJOR_MINOR"
fi