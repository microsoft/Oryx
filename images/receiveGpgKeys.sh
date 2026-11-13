#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

KEY_IDS="$@"

if [ "$KEY_IDS" == "" ]; then
    echo "Error: No keys provided to verify"
    exit 1
fi

for keyID in $KEY_IDS; do
    for i in {1..10}; do
        echo "Try # $i"
        EXIT_CODE=0
        gpg --batch --keyserver hkps://keyserver.ubuntu.com:443 --recv-keys "$keyID" || \
        gpg --batch --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys "$keyID" || \
        gpg --batch --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys "$keyID" || \
        gpg --batch --keyserver hkp://pgp.mit.edu:80 --recv-keys "$keyID" || \
        EXIT_CODE=$?

        if [ "$EXIT_CODE" -eq 0 ]; then
            echo "Received key successfully."
            break
        fi
    done
    if [ "$EXIT_CODE" -ne 0 ]; then
        echo "Error: Unable to receive GPG key $keyID after 10 attempts" >&2
        exit 1
    fi
done