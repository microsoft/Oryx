#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

if [ "$#" -eq 0 ]; then
    echo "Usage: install-pecl.sh package [package ...]" >&2
    exit 2
fi

downloadDir=$(mktemp -d)
trap 'rm -rf -- "$downloadDir"' EXIT
archives=()

for package in "$@"; do
    for attempt in 1 2 3; do
        mkdir "$downloadDir/${#archives[@]}-$attempt"
        cd "$downloadDir/${#archives[@]}-$attempt"
        if pecl download "$package"; then
            files=("$PWD"/*.tgz)
            if [ "${#files[@]}" -ne 1 ] || [ ! -s "${files[0]}" ]; then
                echo "PECL download did not produce one archive for $package" >&2
                exit 1
            fi
            archives+=("${files[0]}")
            break
        else
            status=$?
        fi
        if [ "$attempt" -eq 3 ]; then
            echo "PECL download failed for $package after $attempt attempts" >&2
            exit "$status"
        fi
        echo "PECL download failed for $package (attempt $attempt); retrying" >&2
        sleep "$((attempt * 2))"
    done
done

# Retry only network acquisition, never compilation or installation failures.
pecl install "${archives[@]}"
