#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

if [[ $# -ne 1 || ! -s "$1" ]]; then
    echo "Usage: $0 <image-list>" >&2
    exit 1
fi

while IFS= read -r image; do
    image="${image%$'\r'}"
    [[ -n "$image" ]] || continue
    expected_version="$(sed -n 's/.*:embr-\([0-9]\+\.[0-9]\+\)-ubuntu-resolute-.*/\1/p' <<< "$image")"
    if [[ -z "$expected_version" ]]; then
        echo "Unexpected Embr Python image tag: $image" >&2
        exit 1
    fi

    docker run --rm --env "EXPECTED_VERSION=$expected_version" "$image" sh -ceu '
        actual_version="$(python -c "import sys; print(f\"{sys.version_info.major}.{sys.version_info.minor}\")")"
        test "$actual_version" = "$EXPECTED_VERSION"
        python -c "import bz2, ctypes, curses, dbm.gnu, lzma, readline, sqlite3, ssl, tkinter, uuid"
        python -c "import urllib.request; urllib.request.urlopen(\"https://www.python.org/\", timeout=30).close()"
        test -n "$(find /usr/local/share/ca-certificates -name "azl_*.crt" -print -quit)"
        test ! -x /usr/bin/gcc
        test ! -x /usr/bin/g++
        python -c "import importlib.util; assert importlib.util.find_spec(\"pip\") is None"
        python -c "import importlib.util; assert importlib.util.find_spec(\"gunicorn\") is None"
        python -c "import importlib.util; assert importlib.util.find_spec(\"uvicorn\") is None"
    '
done < "$1"

echo "All Embr Python runtime image tests passed."
