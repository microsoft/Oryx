#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

__CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source "$__CURRENT_DIR/../../build/__hugoConstants.sh"

fileName=$(echo $TAR_FILE_NAME_FORMAT | sed "s/#VERSION#/$VERSION/g")
url=$(echo $INSTALLATION_URL_FORMAT | sed "s/#VERSION#/$VERSION/g")
url=$(echo $url | sed "s/#TAR_FILE#/$fileName/g")
request="curl -fsSLO --compressed $url"
# @retry if curl fails
/opt/tmp/images/retry.sh "$request"
installationDir="$INSTALLED_HUGO_VERSIONS_DIR/${VERSION}"
mkdir -p "$installationDir"
tar -xzf "$fileName" -C "$installationDir"
rm "$fileName"
ln -s "$installationDir" "$INSTALLED_HUGO_VERSIONS_DIR/lts"


