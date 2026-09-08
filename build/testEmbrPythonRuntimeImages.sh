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
        python -c "import bz2, ctypes, curses, dbm.gnu, lzma, readline, sqlite3, ssl, uuid"
        python -c "import urllib.request; urllib.request.urlopen(\"https://www.python.org/\", timeout=30).close()"
        python -c "import ctypes; ctypes.CDLL(\"libpq.so.5\"); ctypes.CDLL(\"libmysqlclient.so.24\")"
        test -n "$(find /usr/local/share/ca-certificates -name "azl_*.crt" -print -quit)"
        test ! -x /usr/bin/gcc
        test ! -x /usr/bin/g++
        python -c "import importlib.util; assert importlib.util.find_spec(\"pip\") is None"
        python -c "import importlib.util; assert importlib.util.find_spec(\"_tkinter\") is None"
        gunicorn --version | grep -q "^gunicorn (version "
        python -c "import importlib.util; assert importlib.util.find_spec(\"uvicorn\") is None"
        test -x /opt/oryx/benv
        test -x /opt/startupcmdgen/startupcmdgen
        test "$(readlink /usr/local/bin/oryx)" = "/opt/startupcmdgen/startupcmdgen"
        mkdir -p /tmp/oryx-startup-test
        oryx create-script \
            -appPath /tmp/oryx-startup-test \
            -output /tmp/oryx-startup.sh \
            -bindPort 8000 \
            -userStartupCommand "gunicorn wsgiref.simple_server:demo_app --bind 0.0.0.0:8000"
        test -s /tmp/oryx-startup.sh
        grep -q "gunicorn wsgiref.simple_server:demo_app" /tmp/oryx-startup.sh
        EXPECTED_VERSION="$EXPECTED_VERSION" bash -c '"'"'
            source /opt/oryx/benv "python=$EXPECTED_VERSION"
            test "$python" = "/opt/python/$EXPECTED_VERSION/bin/python"
            test "$(python -c "import sys; print(f\"{sys.version_info.major}.{sys.version_info.minor}\")")" = "$EXPECTED_VERSION"
        '"'"'
    '
done < "$1"

echo "All Embr Python runtime image tests passed."
