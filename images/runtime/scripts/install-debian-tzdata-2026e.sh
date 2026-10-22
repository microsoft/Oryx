#!/bin/bash

set -euo pipefail

readonly source_file="/etc/apt/sources.list.d/oryx-forky-tzdata.sources"
readonly preferences_file="/etc/apt/preferences.d/oryx-forky-tzdata"
readonly timezone_file_backup="/tmp/oryx-timezone"

rm -f "${timezone_file_backup}"
if [ -f /etc/timezone ]; then
    cp -a /etc/timezone "${timezone_file_backup}"
fi

cat > "${source_file}" <<'EOF'
Types: deb
URIs: http://deb.debian.org/debian
Suites: forky
Components: main
Signed-By: /usr/share/keyrings/debian-archive-keyring.gpg
EOF

cat > "${preferences_file}" <<'EOF'
Package: *
Pin: release n=forky
Pin-Priority: -1

Package: tzdata tzdata-legacy
Pin: release n=forky
Pin-Priority: 1001
EOF

apt-get update
DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    tzdata \
    tzdata-legacy

if [ -f "${timezone_file_backup}" ]; then
    mv "${timezone_file_backup}" /etc/timezone
fi

rm -rf /usr/share/zoneinfo/posix
ln -s . /usr/share/zoneinfo/posix

rm -f "${source_file}" "${preferences_file}"
rm -rf /var/lib/apt/lists/*
