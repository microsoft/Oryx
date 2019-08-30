#!/bin/bash

set -e

PLATFORM_NAME="$1"
VERSION="$2"
fileName="$PLATFORM_NAME-$VERSION.tar.gz"
targetDir="/opt/$PLATFORM_NAME/$VERSION"
ORYX_BLOB_URL_BASE="https://oryxsdks.blob.core.windows.net/sdks/"

echo "Installing $PLATFORM_NAME version '$VERSION'..."
cd /tmp
rm -f "$fileName"
curl -SL $ORYX_BLOB_URL_BASE/$PLATFORM_NAME-$VERSION.tar.gz --output $fileName
mkdir -p "$targetDir"
tar -xzf $fileName -C "$targetDir"
rm -f "$fileName"
