#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

PLATFORM_NAME="$1"
VERSION="$2"
fileName="$PLATFORM_NAME-$VERSION.tar.gz"
platformDir="/opt/$PLATFORM_NAME"
targetDir="$platformDir/$VERSION"
ORYX_BLOB_URL_BASE="https://oryxsdksdev.blob.core.windows.net/$PLATFORM_NAME"

echo
echo "Installing $PLATFORM_NAME version '$VERSION'..."
cd /tmp
rm -f "$fileName"
curl -SL $ORYX_BLOB_URL_BASE/$PLATFORM_NAME-$VERSION.tar.gz --output $fileName
mkdir -p "$targetDir"
tar -xzf $fileName -C "$targetDir"
rm -f "$fileName"

# Create a link : major.minor => major.minor.patch
cd "$platformDir"
IFS='.' read -ra VERSION_PARTS <<< "$VERSION"
MAJOR_MINOR="${VERSION_PARTS[0]}.${VERSION_PARTS[1]}"
echo
echo "Created link from $MAJOR_MINOR to $VERSION"
ln -s "$VERSION" "$MAJOR_MINOR"