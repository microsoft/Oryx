#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

__CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source "$__CURRENT_DIR/../../build/__hugoConstants.sh"

fileName="hugo_extended_${VERSION}_Linux-64bit.tar.gz"
curl -fsSLO --compressed \
    "https://github.com/gohugoio/hugo/releases/download/v${VERSION}/$fileName"
installationDir="/opt/hugo/${VERSION}"
mkdir -p "$installationDir"
tar -xzf "$fileName" -C "$installationDir"
rm "$fileName"
ln -s "$installationDir" "/opt/hugo/lts"


