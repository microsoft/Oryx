#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -eo pipefail

readonly TEST_REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly TEST_WORK_DIR="$(mktemp -d)"
export TEST_WORK_DIR
trap 'rm -rf "$TEST_WORK_DIR"' EXIT

mkdir -p "$TEST_WORK_DIR/build" "$TEST_WORK_DIR/platforms/dotnet/versions/"{noble,resolute}
cp "$TEST_REPO_DIR/build/__variables.sh" "$TEST_REPO_DIR/build/__sdkStorageConstants.sh" "$TEST_WORK_DIR/build"
cp "$TEST_REPO_DIR/platforms/dotnet/getDotNetSdk.sh" "$TEST_WORK_DIR/platforms/dotnet"
cp "$TEST_REPO_DIR/platforms/__common.sh" "$TEST_WORK_DIR/platforms/__common.sh"

# Exercise the real version filtering, replacing only the SDK download callback.
cat >> "$TEST_WORK_DIR/platforms/__common.sh" <<'COMMON'

eval "$(declare -f buildPlatform | sed '1s/buildPlatform/buildTestPlatform/')"
recordSdk() {
    printf '%s|%s|%s|%s\n' "$debianFlavor" "$1" "$2" "$3" >> "$TEST_WORK_DIR/actual"
}
buildPlatform() {
    buildTestPlatform "$1" recordSdk
}
COMMON

cat > "$TEST_WORK_DIR/platforms/dotnet/versions/noble/versionsToBuild.txt" <<'NOBLE'
# version, sha, download url
9.0.321, sha9,
10.0.404, sha10, https://example.invalid/sdk10.tar.gz
NOBLE
cat > "$TEST_WORK_DIR/platforms/dotnet/versions/resolute/versionsToBuild.txt" <<'RESOLUTE'
# version, sha, download url
11.0.100, sha11,
RESOLUTE
printf '%s\n' '10.0.404' > "$TEST_WORK_DIR/platforms/dotnet/versions/noble/defaultVersion.txt"
printf '%s\n' '11.0.100' > "$TEST_WORK_DIR/platforms/dotnet/versions/resolute/defaultVersion.txt"

runSdk() {
    : > "$TEST_WORK_DIR/actual"
    VERSIONS_TO_BUILD_OVERRIDE="$2" bash "$TEST_WORK_DIR/platforms/dotnet/getDotNetSdk.sh" "$1" "" > "$TEST_WORK_DIR/output" 2>&1
}

runSdk noble '10.0.404,11.0.100'
printf '%s\n' 'noble|10.0.404|sha10|https://example.invalid/sdk10.tar.gz' > "$TEST_WORK_DIR/expected"
diff -u "$TEST_WORK_DIR/expected" "$TEST_WORK_DIR/actual"

runSdk resolute '10.0.404,11.0.100'
printf '%s\n' \
    'resolute|10.0.404|sha10|https://example.invalid/sdk10.tar.gz' \
    'resolute|11.0.100|sha11|' > "$TEST_WORK_DIR/expected"
diff -u "$TEST_WORK_DIR/expected" "$TEST_WORK_DIR/actual"
cmp "$TEST_WORK_DIR/platforms/dotnet/versions/resolute/defaultVersion.txt" \
    "$TEST_WORK_DIR/artifacts/platformSdks/dotnet/defaultVersion.resolute.txt"

# A future release only added to Noble must immediately become available on Resolute.
printf '%s\n' '10.0.405, next-sha, https://example.invalid/next-sdk.tar.gz' \
    >> "$TEST_WORK_DIR/platforms/dotnet/versions/noble/versionsToBuild.txt"
runSdk resolute '10.0.405'
printf '%s\n' 'resolute|10.0.405|next-sha|https://example.invalid/next-sdk.tar.gz' > "$TEST_WORK_DIR/expected"
diff -u "$TEST_WORK_DIR/expected" "$TEST_WORK_DIR/actual"

runSdk resolute ''
printf '%s\n' \
    'resolute|10.0.404|sha10|https://example.invalid/sdk10.tar.gz' \
    'resolute|10.0.405|next-sha|https://example.invalid/next-sdk.tar.gz' \
    'resolute|11.0.100|sha11|' > "$TEST_WORK_DIR/expected"
diff -u "$TEST_WORK_DIR/expected" "$TEST_WORK_DIR/actual"
if grep -q '^10[.]' "$TEST_WORK_DIR/platforms/dotnet/versions/resolute/versionsToBuild.txt"; then
    echo "Resolute's checked-in manifest must not be rewritten." >&2
    exit 1
fi

: > "$TEST_WORK_DIR/platforms/dotnet/versions/noble/versionsToBuild.txt"
if runSdk resolute '10.0.405'; then
    echo "Missing shared .NET 10 SDK metadata should fail." >&2
    exit 1
fi
grep -q 'No .NET 10 SDK versions found' "$TEST_WORK_DIR/output"

echo ".NET SDK manifest sharing checks passed."
