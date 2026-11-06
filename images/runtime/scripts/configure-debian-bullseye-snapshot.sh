#!/bin/bash

set -euo pipefail

debian_archive_url="http://archive.debian.org/debian"
security_archive_url="http://archive.debian.org/debian-security"

mapfile -d '' apt_source_files < <(find /etc/apt -type f \( -name '*.list' -o -name '*.sources' \) -print0)
if [ "${#apt_source_files[@]}" -eq 0 ]; then
    echo "No APT source files were found." >&2
    exit 1
fi

sed -i \
    -e '/^[[:space:]]*deb\(-src\)\?[[:space:]]/ { /[[:space:]]bullseye\(-updates\|-security\)\?[[:space:]]/!b; }' \
    -e "s|https\\?://snapshot.debian.org/archive/debian-security/20260831T211305Z|${security_archive_url}|g" \
    -e "s|https\\?://snapshot.debian.org/archive/debian/20260831T211305Z|${debian_archive_url}|g" \
    -e "s|https\\?://deb.debian.org/debian-security|${security_archive_url}|g" \
    -e "s|https\\?://security.debian.org/debian-security|${security_archive_url}|g" \
    -e "s|https\\?://deb.debian.org/debian|${debian_archive_url}|g" \
    "${apt_source_files[@]}"

if ! grep -Eq "${debian_archive_url}(/|[[:space:]]|$)" "${apt_source_files[@]}"; then
    echo "The Debian APT source was not updated to the archive." >&2
    exit 1
fi

if ! grep -Fq "${security_archive_url}" "${apt_source_files[@]}"; then
    echo "The Debian Security APT source was not updated to the archive." >&2
    exit 1
fi

printf '%s\n' 'Acquire::Check-Valid-Until "false";' > /etc/apt/apt.conf.d/99debian-snapshot
rm -rf /var/lib/apt/lists/*
