#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/constants.yml"
readonly EMBR_CONSTANTS_FILE="$REPO_DIR/images/embrconstants.yml"
readonly DOCKERFILE="$REPO_DIR/images/runtime/dotnetcore/embr/template.Dockerfile"
readonly ACR_NAME="${ACR_NAME:-oryxdevmcr.azurecr.io}"
readonly ACR_REPOSITORY="${ACR_REPOSITORY:-public/oryx/dotnetcore}"
readonly RELEASE_TAG_NAME="${RELEASE_TAG_NAME:-local}"
readonly BUILD_DEFINITIONNAME="${BUILD_DEFINITIONNAME:-local}"
readonly ARTIFACTS_DIR="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$REPO_DIR/artifacts}"
readonly IMAGE_LIST="$ARTIFACTS_DIR/images/embr-dotnet-runtime-images-acr.resolute.txt"
readonly BASE_IMAGE="docker.io/library/embr_oryx_run_base_resolute"

yaml_value() {
    local file="$1"
    local key="$2"
    local value
    value="$(sed -n "s/^  ${key}:[[:space:]]*//p" "$file")"
    if [[ -z "$value" ]]; then
        echo "Missing $key in $file." >&2
        return 1
    fi
    printf '%s\n' "$value"
}

os_flavor="$(yaml_value "$EMBR_CONSTANTS_FILE" dotnet100osFlavors)"
if [[ "$os_flavor" != "resolute" ]]; then
    echo "Embr .NET 10 supports only Resolute, not '$os_flavor'." >&2
    exit 1
fi

net_version="$(yaml_value "$CONSTANTS_FILE" NET_CORE_APP_100)"
aspnet_version="$(yaml_value "$CONSTANTS_FILE" ASPNET_CORE_APP_100)"
sdk_version="$(yaml_value "$CONSTANTS_FILE" DOTNET_SDK_100)"
agent_version="$(yaml_value "$CONSTANTS_FILE" USER_DOTNET_AI_VERSION)"

mkdir -p "$(dirname "$IMAGE_LIST")"
: > "$IMAGE_LIST"

docker build \
    --file "$REPO_DIR/images/runtime/commonbase/Dockerfile" \
    --tag "$BASE_IMAGE" \
    --build-arg "OS_FLAVOR=$os_flavor" \
    --build-arg "OS_TYPE=ubuntu" \
    "$REPO_DIR"

secret_args=()
if [[ -n "${VSS_NUGET_ACCESSTOKEN:-}" ]]; then
    secret_args+=(--secret id=vss_nuget_accesstoken,env=VSS_NUGET_ACCESSTOKEN)
fi

image="$ACR_NAME/$ACR_REPOSITORY:embr-10.0-ubuntu-resolute-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
docker build \
    --file "$DOCKERFILE" \
    --tag "$image" \
    "${secret_args[@]}" \
    --build-arg "BASE_IMAGE=$BASE_IMAGE" \
    --build-arg "DOTNET_SDK_VERSION=$sdk_version" \
    --build-arg "NET_CORE_APP_100=$net_version" \
    --build-arg "ASPNET_CORE_APP_100=$aspnet_version" \
    --build-arg "USER_DOTNET_AI_VERSION=$agent_version" \
    --build-arg "VSS_NUGET_URI_PREFIXES=${VSS_NUGET_URI_PREFIXES:-}" \
    --build-arg "VSS_NUGET_EXTERNAL_FEED_ENDPOINTS=${VSS_NUGET_EXTERNAL_FEED_ENDPOINTS:-}" \
    --build-arg "AI_CONNECTION_STRING=${AI_CONNECTION_STRING:-}" \
    --build-arg "BUILD_NUMBER=${BUILD_BUILDNUMBER:-local}" \
    --build-arg "GIT_COMMIT=${BUILD_SOURCEVERSION:-unspecified}" \
    --build-arg "RELEASE_TAG_NAME=$RELEASE_TAG_NAME" \
    "$REPO_DIR"
echo "$image" >> "$IMAGE_LIST"

echo "Built Embr .NET runtime image:"
cat "$IMAGE_LIST"
