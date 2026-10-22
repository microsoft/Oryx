#!/bin/bash

set -euo pipefail

readonly source_file="/etc/apt/sources.list.d/oryx-forky-tzdata.sources"
readonly preferences_file="/etc/apt/preferences.d/oryx-forky-tzdata"

cleanup() {
    rm -f "${source_file}" "${preferences_file}"
    rm -rf /var/lib/apt/lists/*
}
trap cleanup EXIT

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
    tzdata=2026e-1 \
    tzdata-legacy=2026e-1
