#!/bin/bash

set -euo pipefail

snapshot_timestamp="20260831T211305Z"
debian_snapshot_url="http://snapshot.debian.org/archive/debian/${snapshot_timestamp}/"
security_snapshot_url="http://snapshot.debian.org/archive/debian-security/${snapshot_timestamp}/"

mapfile -d '' apt_source_files < <(find /etc/apt -type f \( -name '*.list' -o -name '*.sources' \) -print0)
if [ "${#apt_source_files[@]}" -eq 0 ]; then
    echo "No APT source files were found." >&2
    exit 1
fi

sed -i \
    -e "s|https\\?://deb.debian.org/debian-security|${security_snapshot_url}|g" \
    -e "s|https\\?://security.debian.org/debian-security|${security_snapshot_url}|g" \
    -e "s|https\\?://deb.debian.org/debian|${debian_snapshot_url}|g" \
    "${apt_source_files[@]}"

if ! grep -Fq "${debian_snapshot_url}" "${apt_source_files[@]}"; then
    echo "The Debian APT source was not updated to the snapshot." >&2
    exit 1
fi

if ! grep -Fq "${security_snapshot_url}" "${apt_source_files[@]}"; then
    echo "The Debian Security APT source was not updated to the snapshot." >&2
    exit 1
fi

printf '%s\n' 'Acquire::Check-Valid-Until "false";' > /etc/apt/apt.conf.d/99debian-snapshot
rm -rf /var/lib/apt/lists/*
