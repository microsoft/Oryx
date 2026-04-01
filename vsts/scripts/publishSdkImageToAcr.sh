#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Publishes built SDK tarballs as OCI images to Azure Container Registry (ACR).
# Each SDK version is pushed as a single-layer FROM-scratch OCI image where the
# layer IS the tarball. Metadata is stored as OCI config labels.
#
# Usage:
#   publishSdkImageToAcr.sh <artifactsDir> <acrName> [dryRun]
#
# Arguments:
#   artifactsDir - Directory containing platformSdks/<platform>/*.tar.gz
#   acrName      - ACR name (e.g., oryxsdks) without .azurecr.io suffix
#   dryRun       - Optional: "True" to only print what would be pushed
#
# Environment variables:
#   BUILD_BUILDNUMBER     - ADO build number for metadata
#   BUILD_SOURCEVERSION   - Git commit SHA for metadata
#   BUILD_SOURCEBRANCH    - Git branch for metadata
#
# Prerequisites:
#   - oras CLI installed (https://oras.land)
#   - az CLI logged in with push access to the ACR
#   - OR: ORAS_USERNAME / ORAS_PASSWORD set for direct auth

set -euo pipefail

ARTIFACTS_DIR="${1:?Missing artifactsDir argument}"
ACR_NAME="${2:?Missing acrName argument}"
DRY_RUN="${3:-False}"

ACR_REGISTRY="${ACR_NAME}.azurecr.io"
REPO_PREFIX="sdks"

PLATFORMS="nodejs python dotnet php php-composer ruby java maven golang"
DEBIAN_FLAVORS="bookworm bullseye buster focal-scm noble"

BUILD_NUMBER="${BUILD_BUILDNUMBER:-local}"
COMMIT="${BUILD_SOURCEVERSION:-unknown}"
BRANCH="${BUILD_SOURCEBRANCH:-unknown}"

echo "========================================"
echo "ACR SDK Image Publisher"
echo "========================================"
echo "Artifacts Dir : $ARTIFACTS_DIR"
echo "ACR Registry  : $ACR_REGISTRY"
echo "Repo Prefix   : $REPO_PREFIX"
echo "Dry Run       : $DRY_RUN"
echo "Build Number  : $BUILD_NUMBER"
echo "Commit        : $COMMIT"
echo "Branch        : $BRANCH"
echo "========================================"

# Ensure oras is installed
if ! command -v oras &> /dev/null; then
    echo "Installing oras CLI..."
    ORAS_VERSION="1.2.0"
    curl -sSLO "https://github.com/oras-project/oras/releases/download/v${ORAS_VERSION}/oras_${ORAS_VERSION}_linux_amd64.tar.gz"
    tar -xzf "oras_${ORAS_VERSION}_linux_amd64.tar.gz" -C /usr/local/bin/ oras
    rm -f "oras_${ORAS_VERSION}_linux_amd64.tar.gz"
fi

echo "oras version: $(oras version)"

# Login to ACR if not already logged in
if [ -z "${ORAS_USERNAME:-}" ]; then
    echo "Logging in to ACR via az CLI..."
    az acr login --name "$ACR_NAME" 2>/dev/null || true
fi

PUSH_COUNT=0
SKIP_COUNT=0
ERROR_COUNT=0

for platform in $PLATFORMS; do
    platformDir="$ARTIFACTS_DIR/platformSdks/$platform"
    if [ ! -d "$platformDir" ]; then
        echo "No artifacts found for platform: $platform (skipping)"
        continue
    fi

    echo ""
    echo "Processing platform: $platform"
    echo "----------------------------------------"

    for tarball in "$platformDir"/*.tar.gz; do
        [ -f "$tarball" ] || continue

        filename=$(basename "$tarball")

        # Parse version and debian flavor from filename
        # Expected formats:
        #   <platform>-<version>.tar.gz                  (stretch/default)
        #   <platform>-<debianFlavor>-<version>.tar.gz   (with flavor prefix)
        baseName="${filename%.tar.gz}"

        # Try to extract debian flavor and version
        debianFlavor=""
        version=""

        for flavor in $DEBIAN_FLAVORS; do
            if [[ "$baseName" == *"-${flavor}-"* ]]; then
                debianFlavor="$flavor"
                # Extract version after the flavor
                version="${baseName##*-${flavor}-}"
                break
            fi
        done

        if [ -z "$debianFlavor" ]; then
            # No flavor prefix - assume default (bookworm) or extract version directly
            # Format: <platform>-<version>.tar.gz
            version="${baseName#${platform}-}"
            debianFlavor="bookworm"
        fi

        if [ -z "$version" ]; then
            echo "WARNING: Could not parse version from $filename — skipping"
            continue
        fi

        repository="${REPO_PREFIX}/${platform}"
        tag="${debianFlavor}-${version}"
        fullRef="${ACR_REGISTRY}/${repository}:${tag}"

        # Compute SHA256 checksum
        sha256=$(sha256sum "$tarball" | cut -d' ' -f1)

        echo ""
        echo "  File    : $filename"
        echo "  Version : $version"
        echo "  Flavor  : $debianFlavor"
        echo "  Tag     : $tag"
        echo "  Ref     : $fullRef"
        echo "  SHA256  : $sha256"

        # Read metadata file if it exists
        metadataFile="${tarball%.tar.gz}-metadata.txt"
        extraAnnotations=""
        if [ -f "$metadataFile" ]; then
            while IFS='=' read -r key value; do
                [ -z "$key" ] && continue
                [[ "$key" == \#* ]] && continue
                extraAnnotations="$extraAnnotations --annotation \"org.opencontainers.image.${key}=${value}\""
            done < "$metadataFile"
        fi

        if [ "$DRY_RUN" = "True" ]; then
            echo "  [DRY RUN] Would push: $fullRef"
            SKIP_COUNT=$((SKIP_COUNT + 1))
            continue
        fi

        # Push SDK tarball as OCI artifact using oras
        echo "  Pushing to ACR..."
        if oras push "$fullRef" \
            --artifact-type "application/vnd.oryx.sdk.layer.v1.tar+gzip" \
            --annotation "org.oryx.sdk.version=${version}" \
            --annotation "org.oryx.sdk.platform=${platform}" \
            --annotation "org.oryx.sdk.os-flavor=${debianFlavor}" \
            --annotation "org.oryx.sdk.sha256=${sha256}" \
            --annotation "org.oryx.sdk.build-number=${BUILD_NUMBER}" \
            --annotation "org.oryx.sdk.commit=${COMMIT}" \
            --annotation "org.oryx.sdk.branch=${BRANCH}" \
            "$tarball:application/vnd.oryx.sdk.layer.v1.tar+gzip" 2>&1; then
            echo "  Successfully pushed: $fullRef"
            PUSH_COUNT=$((PUSH_COUNT + 1))
        else
            echo "  ERROR: Failed to push: $fullRef"
            ERROR_COUNT=$((ERROR_COUNT + 1))
        fi
    done

    # Handle defaultVersion files — push as a special tag
    for defaultFile in "$platformDir"/defaultVersion*.txt; do
        [ -f "$defaultFile" ] || continue

        defaultFileName=$(basename "$defaultFile")

        # Parse debian flavor from defaultVersion.<flavor>.txt
        if [[ "$defaultFileName" == "defaultVersion."*".txt" ]]; then
            debianFlavor="${defaultFileName#defaultVersion.}"
            debianFlavor="${debianFlavor%.txt}"
        else
            debianFlavor="bookworm"
        fi

        defaultVersion=$(cat "$defaultFile" | tr -d '[:space:]')
        if [ -z "$defaultVersion" ]; then
            echo "  WARNING: Empty default version file: $defaultFileName"
            continue
        fi

        repository="${REPO_PREFIX}/${platform}"
        defaultTag="${debianFlavor}-default"
        fullRef="${ACR_REGISTRY}/${repository}:${defaultTag}"

        echo ""
        echo "  Default version for $debianFlavor: $defaultVersion"
        echo "  Tag: $defaultTag -> $fullRef"

        if [ "$DRY_RUN" = "True" ]; then
            echo "  [DRY RUN] Would push default version config: $fullRef"
            continue
        fi

        # Create a minimal config JSON with the default version as a label
        tmpConfig=$(mktemp)
        cat > "$tmpConfig" <<EOF
{
  "org.oryx.sdk.default-version": "${defaultVersion}",
  "org.oryx.sdk.platform": "${platform}",
  "org.oryx.sdk.os-flavor": "${debianFlavor}"
}
EOF

        if oras push "$fullRef" \
            --artifact-type "application/vnd.oryx.sdk.default-version.v1+json" \
            --annotation "org.oryx.sdk.default-version=${defaultVersion}" \
            --annotation "org.oryx.sdk.platform=${platform}" \
            --annotation "org.oryx.sdk.os-flavor=${debianFlavor}" \
            "$tmpConfig:application/json" 2>&1; then
            echo "  Successfully pushed default version tag: $fullRef"
        else
            echo "  ERROR: Failed to push default version tag: $fullRef"
            ERROR_COUNT=$((ERROR_COUNT + 1))
        fi

        rm -f "$tmpConfig"
    done
done

echo ""
echo "========================================"
echo "Summary"
echo "========================================"
echo "Pushed : $PUSH_COUNT"
echo "Skipped: $SKIP_COUNT"
echo "Errors : $ERROR_COUNT"
echo "========================================"

if [ "$ERROR_COUNT" -gt 0 ]; then
    echo "WARNING: $ERROR_COUNT errors occurred during publishing."
    exit 1
fi

echo "Done."
