#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

KEYS="$@"

if [ "$KEYS" == "" ]; then
    echo "Error: No keys provided to verify"
    exit 1
fi

for key in $KEYS; do
    for i in {1..10}; do
        echo "Try # $i"
        gpg --batch --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys "$key" || \
        gpg --batch --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys "$key" || \
        gpg --batch --keyserver hkp://pgp.mit.edu:80 --recv-keys "$key" || \
        EXIT_CODE=$?

        if [ "$EXIT_CODE" == '' ] || [ "$EXIT_CODE" == 0 ]; then
            echo "Received key successfully."
            break
        fi
    done
done