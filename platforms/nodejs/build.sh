#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

version="$1"
osFlavor="$OS_FLAVOR"

tarFileName=nodejs-$osFlavor-$version.tar.gz
nodeFileName="node-v${version}-linux-x64.tar.xz"

NODE_DOWNLOAD_URL="https://nodejs.org/dist/v${version}/${nodeFileName}"
NODE_SHASUM_ASC_URL="https://nodejs.org/dist/v${version}/SHASUMS256.txt.asc"
NODE_KEYRING_URL="https://github.com/nodejs/release-keys/raw/HEAD/gpg/pubring.kbx"

INSTALL_DIR="/opt/nodejs/${version}"
WORK_DIR="/tmp/node-download"
mkdir -p "$INSTALL_DIR" "$WORK_DIR"
cd "$WORK_DIR"

echo "Downloading Node.js v${version} from official source..."
curl -fsSLO "$NODE_DOWNLOAD_URL"
curl -fsSL "$NODE_SHASUM_ASC_URL" -o SHASUMS256.txt.asc
curl -fsSL "$NODE_KEYRING_URL" -o nodejs-keyring.kbx

# Verify GPG signature and extract verified checksums from clearsigned file
gpg --no-default-keyring --keyring="$WORK_DIR/nodejs-keyring.kbx" --decrypt SHASUMS256.txt.asc > SHASUMS256.txt

# Verify SHA256 integrity of the downloaded tarball
grep "$nodeFileName" SHASUMS256.txt > node.sha256
[ -s node.sha256 ] || { echo "ERROR: $nodeFileName not found in SHASUMS256.txt"; exit 1; }
sha256sum -c node.sha256

# Extract to install directory
tar -xJf "$nodeFileName" -C "$INSTALL_DIR" --strip-components=1
rm -rf "$WORK_DIR"

cd "$INSTALL_DIR"
mkdir -p /tmp/compressedSdk/nodejs
tar -zcf /tmp/compressedSdk/nodejs/$tarFileName .
