#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

version="$1"
osFlavor="$OS_FLAVOR"

tarFileName=nodejs-$osFlavor-$version.tar.gz
nodeFileName="node-v${version}-linux-x64.tar.xz"

NODE_DOWNLOAD_URL="https://nodejs.org/dist/v${version}/${nodeFileName}"
NODE_SHASUM_URL="https://nodejs.org/dist/v${version}/SHASUMS256.txt"

INSTALL_DIR="/opt/nodejs/${version}"
mkdir -p "$INSTALL_DIR"

echo "Downloading Node.js v${version} from official source..."
curl -fsSLO "$NODE_DOWNLOAD_URL"
curl -fsSL "$NODE_SHASUM_URL" -o SHASUMS256.txt

# Verify SHA256 integrity
grep "$nodeFileName" SHASUMS256.txt | sha256sum -c -

# Extract to install directory
tar -xJf "$nodeFileName" -C "$INSTALL_DIR" --strip-components=1
rm -f "$nodeFileName" SHASUMS256.txt

cd "$INSTALL_DIR"
mkdir -p /tmp/compressedSdk/nodejs
tar -zcf /tmp/compressedSdk/nodejs/$tarFileName .
