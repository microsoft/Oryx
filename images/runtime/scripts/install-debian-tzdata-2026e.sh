#!/bin/bash

set -euo pipefail

readonly source_file="/etc/apt/sources.list.d/oryx-forky-tzdata.sources"
readonly preferences_file="/etc/apt/preferences.d/oryx-forky-tzdata"

cleanup() {
    rm -f "${source_file}" "${preferences_file}"
    rm -rf /var/lib/apt/lists/*
}
trap cleanup EXIT

timezone="$(readlink /etc/localtime 2>/dev/null || cat /etc/timezone 2>/dev/null || printf '%s\n' 'Etc/UTC')"
timezone="${timezone#/usr/share/zoneinfo/}"
timezone="${timezone#../usr/share/zoneinfo/}"
if [ -z "${timezone}" ] || [[ "${timezone}" = /* ]] || [[ "${timezone}" = *".."* ]]; then
    echo "Unable to preserve the currently configured timezone: ${timezone}" >&2
    exit 1
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
    tzdata=2026e-1 \
    tzdata-legacy=2026e-1

test -e "/usr/share/zoneinfo/${timezone}"
ln -snf "/usr/share/zoneinfo/${timezone}" /etc/localtime
printf '%s\n' "${timezone}" > /etc/timezone

test -d /usr/share/zoneinfo/posix
test "$(TZ=America/Winnipeg date --date='2026-12-01 12:00:00' +%z)" = "-0500"
test "$(TZ=America/Edmonton date --date='2026-12-01 12:00:00' +%z)" = "-0600"
