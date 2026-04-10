#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
# Validates .NET SHA512 hashes against official Microsoft releases.
set -euo pipefail

CONSTANTS_FILE="${1:-images/constants.yml}"

echo "Validating .NET SHA512 hashes..."
failed=0

# Check both runtime types
for prefix in NET_CORE_APP ASPNET_CORE_APP; do
    while IFS=: read -r key version; do
        key=$(echo "$key" | tr -d ' ')
        version=$(echo "$version" | tr -d ' "')
        major_raw="${key##*_}"
        
        # Convert 80 -> 8.0, 100 -> 10.0
        if [[ ${#major_raw} -eq 2 ]]; then
            major="${major_raw:0:1}.${major_raw:1}"
        else
            major="${major_raw:0:2}.${major_raw:2}"
        fi
        
        # Get local SHA - use exact match with leading space to avoid ASPNET matching NET
        local_sha=$(grep -E "^[[:space:]]+${key}_SHA:" "$CONSTANTS_FILE" | awk '{print $2}' || true)
        [[ -n "$local_sha" ]] || { echo "  $key: SKIP (no SHA defined)"; continue; }
        
        # Fetch official SHA
        jq_filter=$([ "$prefix" = "NET_CORE_APP" ] && echo '.releases[].runtime' || echo '.releases[]."aspnetcore-runtime"')
        tarball=$([ "$prefix" = "NET_CORE_APP" ] && echo 'dotnet-runtime-' || echo 'aspnetcore-runtime-')
        
        official=$(curl -sf --max-time 30 \
            "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/${major}/releases.json" | \
            jq -r --arg v "$version" "${jq_filter} | select(.version==\$v) | .files[] | select(.rid==\"linux-x64\" and (.name | contains(\"${tarball}\")) and (.name | endswith(\".tar.gz\"))) | .hash // empty" 2>/dev/null | head -1 || true)
        
        label=$([ "$prefix" = "NET_CORE_APP" ] && echo ".NET Runtime" || echo "ASP.NET Core")
        if [[ -z "$official" ]]; then
            echo "  $label $version: WARN (could not fetch)"
        elif [[ "${official,,}" != "${local_sha,,}" ]]; then
            echo "  $label $version: FAILED"
            echo "    expected: $official"
            echo "    got:      $local_sha"
            failed=1
        else
            echo "  $label $version: OK"
        fi
    done < <(grep -E "^[[:space:]]+${prefix}_[0-9]+:" "$CONSTANTS_FILE")
done

[[ $failed -eq 0 ]] && echo "PASSED" || { echo "FAILED"; exit 1; }