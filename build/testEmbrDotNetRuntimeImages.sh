#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly CONSTANTS_FILE="$REPO_DIR/images/constants.yml"

if [[ $# -ne 1 || ! -s "$1" ]]; then
    echo "Usage: $0 <image-list>" >&2
    exit 1
fi

sdk_version="$(sed -n 's/^  DOTNET_SDK_100:[[:space:]]*//p' "$CONSTANTS_FILE")"
net_version="$(sed -n 's/^  NET_CORE_APP_100:[[:space:]]*//p' "$CONSTANTS_FILE")"
aspnet_version="$(sed -n 's/^  ASPNET_CORE_APP_100:[[:space:]]*//p' "$CONSTANTS_FILE")"
if [[ -z "$sdk_version" || -z "$net_version" || -z "$aspnet_version" ]]; then
    echo "Missing .NET 10 version metadata in $CONSTANTS_FILE." >&2
    exit 1
fi

tested_image=false
while IFS= read -r image || [[ -n "$image" ]]; do
    image="${image%$'\r'}"
    [[ -n "$image" ]] || continue
    if [[ "$image" != *:embr-10.0-ubuntu-resolute-?* ]]; then
        echo "Unexpected Embr .NET image tag: $image" >&2
        exit 1
    fi

    test_image="${image}-smoke-test"
    docker build \
        --file - \
        --tag "$test_image" \
        --build-arg "RUNTIME_IMAGE=$image" \
        --build-arg "DOTNET_SDK_VERSION=$sdk_version" \
        "$REPO_DIR" <<'DOCKERFILE'
ARG RUNTIME_IMAGE
ARG DOTNET_SDK_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
WORKDIR /src
RUN dotnet new web --name EmbrSmoke --output . --framework net10.0 --no-restore \
    && printf '%s\n' \
        'using System.Globalization;' \
        'var app = WebApplication.Create(args);' \
        'app.MapGet("/", () => CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.GetMonthName(1));' \
        'app.Run();' > Program.cs \
    && dotnet publish --configuration Release --output /app /p:UseAppHost=false
FROM ${RUNTIME_IMAGE}
COPY --from=build /app /tmp/embr-dotnet-smoke
DOCKERFILE
    trap 'docker image rm "$test_image" >/dev/null' EXIT

    docker run --rm \
        --env "EXPECTED_NET_VERSION=$net_version" \
        --env "EXPECTED_ASPNET_VERSION=$aspnet_version" \
        "$test_image" bash -ceu '
        . /etc/os-release
        test "$VERSION_CODENAME" = resolute
        test "$DOTNET_VERSION" = 10.0
        runtimes="$(dotnet --list-runtimes)"
        grep -Fq "Microsoft.NETCore.App $EXPECTED_NET_VERSION [" <<< "$runtimes"
        grep -Fq "Microsoft.AspNetCore.App $EXPECTED_ASPNET_VERSION [" <<< "$runtimes"
        test -z "$(dotnet --list-sdks)"
        ! ldd /usr/share/dotnet/dotnet | grep -q "not found"
        test -n "$(find /usr/local/share/ca-certificates -name "azl_*.crt" -print -quit)"
        curl --fail --silent --show-error --max-time 30 \
            "https://builds.dotnet.microsoft.com/dotnet/Runtime/$EXPECTED_NET_VERSION/dotnet-runtime-$EXPECTED_NET_VERSION-linux-x64.tar.gz.sha512" >/dev/null
        test -d /DotNetCoreAgent
        test -x /opt/startupcmdgen/startupcmdgen
        test "$(readlink /usr/local/bin/oryx)" = /opt/startupcmdgen/startupcmdgen
        oryx create-script \
            -appPath /tmp/embr-dotnet-smoke \
            -output /tmp/embr-dotnet-startup.sh \
            -bindPort 8123
        grep -q "EmbrSmoke.dll" /tmp/embr-dotnet-startup.sh
        bash /tmp/embr-dotnet-startup.sh >/tmp/embr-dotnet-startup.log 2>&1 &
        server_pid=$!
        trap "kill $server_pid" EXIT
        for attempt in $(seq 1 30); do
            if test "$(curl --silent --fail http://127.0.0.1:8123)" = janvier; then
                break
            fi
            if ! kill -0 "$server_pid" 2>/dev/null; then
                cat /tmp/embr-dotnet-startup.log >&2
                exit 1
            fi
            sleep 1
        done
        if test "$(curl --silent --fail http://127.0.0.1:8123)" != janvier; then
            cat /tmp/embr-dotnet-startup.log >&2
            exit 1
        fi
        kill "$server_pid"
        wait "$server_pid" || true
        trap - EXIT
    '
    docker image rm "$test_image" >/dev/null
    trap - EXIT
    tested_image=true
done < "$1"

if [[ "$tested_image" != true ]]; then
    echo "No Embr .NET runtime images found in $1." >&2
    exit 1
fi

echo "All Embr .NET runtime image tests passed."
