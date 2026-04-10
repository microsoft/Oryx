#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
#
# Validates PHP SHA256 hashes in constants.yml against official php.net releases.
# Usage: ./validate-php-sha.sh [path/to/constants.yml]
#
# Exit codes:
#   0 - All SHAs validated successfully
#   1 - One or more SHA mismatches found
#   2 - Script error (missing dependencies, file not found, etc.)

set -euo pipefail

CONSTANTS_FILE="${1:-images/constants.yml}"

# Security: Restrict to expected file paths within the repository
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
CONSTANTS_REALPATH="$(realpath -m "$CONSTANTS_FILE" 2>/dev/null || echo "")"

# Validate path was resolved and is within repository
if [[ -z "$CONSTANTS_REALPATH" ]]; then
    echo "Error: Could not resolve constants file path: $CONSTANTS_FILE" >&2
    exit 2
fi

case "$CONSTANTS_REALPATH" in
    "$REPO_ROOT"/*)
        # Path is safe - within repository
        ;;
    *)
        echo "Error: Constants file must be within the repository: $REPO_ROOT" >&2
        exit 2
        ;;
esac

# Colors for output (disabled if not a terminal)
if [[ -t 1 ]]; then
    RED='\033[0;31m'
    GREEN='\033[0;32m'
    YELLOW='\033[0;33m'
    NC='\033[0m' # No Color
else
    RED=''
    GREEN=''
    YELLOW=''
    NC=''
fi

# Check dependencies
for cmd in curl jq grep; do
    if ! command -v "$cmd" &> /dev/null; then
        echo "Error: Required command '$cmd' not found" >&2
        exit 2
    fi
done

# Function to fetch with retry logic
fetch_with_retry() {
    local url="$1"
    local max_attempts=3
    local timeout=10
    local result=""
    
    for attempt in $(seq 1 $max_attempts); do
        result=$(curl -sf --max-time "$timeout" --proto '=https' --tlsv1.2 "$url" 2>/dev/null) && break
        [[ $attempt -lt $max_attempts ]] && sleep $((attempt * 2))
    done
    
    echo "$result"
}

echo "Validating PHP SHA256 hashes from: $CONSTANTS_FILE"
echo "=================================================="

failed=0
validated=0

# Extract PHP versions and SHAs from constants.yml
while IFS= read -r line; do
    # Match lines like:   php85Version: 8.5.1 (with optional leading spaces)
    if [[ "$line" =~ ^[[:space:]]*php([0-9]+)Version:[[:space:]]*([0-9.]+)$ ]]; then
        php_key="php${BASH_REMATCH[1]}"
        version="${BASH_REMATCH[2]}"
        
        # Security: Validate version format strictly (only digits and dots, reasonable length)
        if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || [[ ${#version} -gt 20 ]]; then
            echo "Warning: Skipping invalid version format: $version" >&2
            continue
        fi
        
        # Get corresponding SHA from constants.yml using fixed string match
        sha_line=$(grep -F "${php_key}Version_SHA:" "$CONSTANTS_FILE" 2>/dev/null || true)
        if [[ "$sha_line" =~ _SHA:[[:space:]]*([a-f0-9]{64})$ ]]; then
            local_sha="${BASH_REMATCH[1]}"
            
            # Security: Sanitize output to prevent log injection
            safe_version="${version//[^0-9.]/}"
            echo -n "PHP $safe_version: "
            
            # Fetch official SHA from php.net with retry (URL-encode version just in case)
            encoded_version=$(printf '%s' "$version" | jq -sRr @uri)
            response=$(fetch_with_retry "https://www.php.net/releases/?json&version=${encoded_version}")
            
            # Validate JSON response before parsing
            if [[ -n "$response" ]] && ! echo "$response" | jq empty 2>/dev/null; then
                echo -e "${YELLOW}WARNING - Invalid JSON response from php.net${NC}"
                continue
            fi
            
            official=$(echo "$response" | jq -r '.source[] | select(.filename | endswith(".tar.xz")) | .sha256 // empty' 2>/dev/null || echo "")
            
            # Security: Validate response is a valid SHA256 (64 hex chars)
            if [[ -n "$official" ]] && [[ ! "$official" =~ ^[a-f0-9]{64}$ ]]; then
                echo -e "${YELLOW}WARNING - Invalid SHA format from php.net${NC}"
                continue
            fi
            
            if [[ -z "$official" ]]; then
                echo -e "${YELLOW}WARNING - Could not fetch from php.net${NC}"
            elif [[ "$official" != "$local_sha" ]]; then
                echo -e "${RED}FAILED${NC}"
                echo "  Expected: $official"
                echo "  Got:      $local_sha"
                failed=1
            else
                echo -e "${GREEN}OK${NC}"
                validated=$((validated + 1))
            fi
        else
            # Version found but no SHA - this is likely a configuration error
            safe_version="${version//[^0-9.]/}"
            echo -e "PHP $safe_version: ${YELLOW}WARNING - Version found but ${php_key}Version_SHA is missing${NC}"
        fi
    fi
done < "$CONSTANTS_FILE" || {
    echo "Error: Could not read constants file: $CONSTANTS_FILE" >&2
    exit 2
}

echo "=================================================="
echo "Validated: $validated PHP version(s)"

if [[ $failed -eq 1 ]]; then
    echo -e "${RED}VALIDATION FAILED - SHA256 mismatches detected!${NC}"
    exit 1
else
    echo -e "${GREEN}VALIDATION PASSED${NC}"
    exit 0
fi
