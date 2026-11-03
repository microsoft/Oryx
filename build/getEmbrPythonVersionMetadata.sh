#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/constants.yml"

if [[ $# -lt 1 || $# -gt 2 || ! "$1" =~ ^[0-9]+\.[0-9]+$ || ( $# -eq 2 && "$2" != "--azure-pipelines" ) ]]; then
    echo "Usage: $0 <major.minor> [--azure-pipelines]" >&2
    exit 1
fi

compact_version="${1//./}"
full_version="$(sed -n "s/^  python${compact_version}Version:[[:space:]]*//p" "$CONSTANTS_FILE")"
os_flavors="$(sed -n "s/^  python${compact_version}osFlavors:[[:space:]]*//p" "$CONSTANTS_FILE")"

if [[ -z "$full_version" || -z "$os_flavors" ]]; then
    echo "Missing version or OS mapping for Python $1 in $CONSTANTS_FILE." >&2
    exit 1
fi

# CPython source verification metadata is independent of the build OS.
metadata_file="$REPO_DIR/platforms/python/versions/${os_flavors%%,*}/versionsToBuild.txt"
metadata="$(awk -F, -v version="$full_version" '
    {
        for (i = 1; i <= NF; i++) {
            gsub(/^[[:space:]]+|[[:space:]]+$/, "", $i)
        }
        if ($1 == version) {
            print $2 "|" $3
            count++
        }
    }
    END {
        if (count != 1) {
            print "Expected exactly one verification entry for Python " version " in " FILENAME > "/dev/stderr"
            exit 1
        }
    }
' "$metadata_file")"
IFS='|' read -r gpg_keys sha256 <<< "$metadata"

if [[ -z "$gpg_keys" && -z "$sha256" ]]; then
    echo "Missing GPG keys and SHA256 for Python $full_version in $metadata_file." >&2
    exit 1
fi
if [[ -n "$sha256" && ! "$sha256" =~ ^[[:xdigit:]]{64}$ ]]; then
    echo "Invalid SHA256 for Python $full_version in $metadata_file." >&2
    exit 1
fi
for key in $gpg_keys; do
    if [[ ! "$key" =~ ^[[:xdigit:]]{40}$ ]]; then
        echo "Invalid GPG fingerprint for Python $full_version in $metadata_file." >&2
        exit 1
    fi
done

if [[ "${2:-}" = "--azure-pipelines" ]]; then
    printf '##vso[task.setvariable variable=PYTHON_FULL_VERSION]%s\n' "$full_version"
    printf '##vso[task.setvariable variable=PYTHON_GPG_KEYS]%s\n' "$gpg_keys"
    printf '##vso[task.setvariable variable=PYTHON_SHA256]%s\n' "$sha256"
else
    printf '%s|%s|%s\n' "$full_version" "$gpg_keys" "$sha256"
fi
