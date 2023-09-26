#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
__CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source $__CURRENT_DIR/__sdkStorageConstants.sh

function downloadFileAndVerifyChecksum() {
    set +x
    local platformName="$1"
    local version="$2"
    local downloadedFileName="$3"
    local downloadableFileName="$3"
    local sdkStorageAccountUrl="$4"
    local sasToken="$5"
    local headersFile="/tmp/headers.txt"

    echo "Downloading $platformName version '$version' from '$sdkStorageAccountUrl'..."
    request="curl 
        -D $headersFile 
        -SL $sdkStorageAccountUrl/$platformName/$downloadableFileName$sasToken
        --output $downloadedFileName"
    $__CURRENT_DIR/retry.sh "$request"
    # Use all lowercase letters to find the header and it's value
    headerName="x-ms-meta-checksum"
    # Search the header ignoring case
    checksumHeader=$(cat $headersFile | grep -i $headerName: | tr -d '\r')
    # Change the found header and value to lowercase
    checksumHeader=$(echo $checksumHeader | tr '[A-Z]' '[a-z]')
    checksumValue=${checksumHeader#"$headerName: "}
    rm -f $headersFile
    echo
    echo "Verifying checksum..."
    checksumcode="sha512sum"
    if [ "$platformName" == "golang" ];then
        checksumcode="sha256sum"
    fi
    echo "$checksumValue $downloadedFileName" | $checksumcode -c -
    set -x
}