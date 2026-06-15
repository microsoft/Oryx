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
NODE_SHASUM_ASC_URL="https://nodejs.org/dist/v${version}/SHASUMS256.txt.asc"
NODE_KEYRING_URL="https://github.com/nodejs/release-keys/raw/HEAD/gpg/pubring.kbx"

INSTALL_DIR="/opt/nodejs/${version}"
mkdir -p "$INSTALL_DIR"

echo "Downloading Node.js v${version} from official source..."
curl -fsSLO "$NODE_DOWNLOAD_URL"
curl -fsSL "$NODE_SHASUM_URL" -o SHASUMS256.txt
curl -fsSL "$NODE_SHASUM_ASC_URL" -o SHASUMS256.txt.asc
curl -fsSL "$NODE_KEYRING_URL" -o nodejs-keyring.kbx

# Verify GPG signature of SHASUMS256.txt
gpgv --keyring="$(pwd)/nodejs-keyring.kbx" SHASUMS256.txt.asc SHASUMS256.txt

# Verify SHA256 integrity of the downloaded tarball
grep "$nodeFileName" SHASUMS256.txt | sha256sum -c -

# Extract to install directory
tar -xJf "$nodeFileName" -C "$INSTALL_DIR" --strip-components=1
rm -f "$nodeFileName" SHASUMS256.txt SHASUMS256.txt.asc nodejs-keyring.kbx

cd "$INSTALL_DIR"
mkdir -p /tmp/compressedSdk/nodejs
tar -zcf /tmp/compressedSdk/nodejs/$tarFileName .
