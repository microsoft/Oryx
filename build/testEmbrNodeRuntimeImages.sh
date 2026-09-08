#!/usr/bin/env bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail

readonly REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly NATIVE_MODULE_TEST_DOCKERFILE="$REPO_DIR/images/runtime/node/embr/nativeModuleTest.Dockerfile"

if [[ $# -ne 1 || ! -s "$1" ]]; then
    echo "Usage: $0 <image-list>" >&2
    exit 1
fi

while IFS= read -r image; do
    image="${image%$'\r'}"
    [[ -n "$image" ]] || continue
    expected_major="$(sed -n 's/.*:\([0-9]\+\)-resolute$/\1/p' <<< "$image")"
    if [[ -z "$expected_major" ]]; then
        echo "Unexpected Embr Node image tag: $image" >&2
        exit 1
    fi

    docker run --rm --env "EXPECTED_MAJOR=$expected_major" "$image" bash -ceu '
        test "$(node -p "process.versions.node")" = "$NODE_VERSION"
        test "$(node -p "process.versions.node.split(\".\")[0]")" = "$EXPECTED_MAJOR"
        npm --version >/dev/null
        npx --version >/dev/null
        test "$(yarn --version)" = "$YARN_VERSION"
        node -e "require(\"https\").get(\"https://nodejs.org/\", response => { if (response.statusCode >= 400) process.exit(1); response.resume(); }).on(\"error\", () => process.exit(1))"
        test -n "$(find /usr/local/share/ca-certificates -name "azl_*.crt" -print -quit)"
        for excluded_command in gcc g++ cc make python python3 git pm2 corepack; do
            ! command -v "$excluded_command" >/dev/null
        done
        command -v tar >/dev/null
        command -v gzip >/dev/null
        command -v unzip >/dev/null
        command -v zstd >/dev/null
        test -x /opt/oryx/benv
        test -x /opt/startupcmdgen/startupcmdgen
        test "$(readlink /usr/local/bin/oryx)" = "/opt/startupcmdgen/startupcmdgen"
        ! find /opt/nodejs -type f \( -name "*.h" -o -name "*.a" \) -print -quit | grep -q .
        ! ldd "$(command -v node)" | grep -q "not found"

        mkdir -p /tmp/npm-app
        cat > /tmp/npm-app/package.json <<EOF
{"scripts":{"start":"node server.js"}}
EOF
        cat > /tmp/npm-app/server.js <<EOF
require("http").createServer((request, response) => response.end("ok")).listen(process.env.PORT);
EOF
        oryx create-script -appPath /tmp/npm-app -output /tmp/npm-startup.sh -bindPort 8123
        grep -q "npm start" /tmp/npm-startup.sh
        sh /tmp/npm-startup.sh >/tmp/npm-startup.log 2>&1 &
        server_pid=$!
        trap "kill $server_pid 2>/dev/null || true" EXIT
        for attempt in $(seq 1 30); do
            if test "$(curl --silent --fail http://127.0.0.1:8123)" = "ok"; then
                break
            fi
            if ! kill -0 "$server_pid" 2>/dev/null; then
                cat /tmp/npm-startup.log >&2
                exit 1
            fi
            sleep 1
        done
        test "$(curl --silent --fail http://127.0.0.1:8123)" = "ok"
        kill "$server_pid"
        wait "$server_pid" || true
        trap - EXIT

        mkdir -p /tmp/yarn-app
        cp /tmp/npm-app/package.json /tmp/npm-app/server.js /tmp/yarn-app/
        touch /tmp/yarn-app/yarn.lock
        oryx create-script -appPath /tmp/yarn-app -output /tmp/yarn-startup.sh -bindPort 8124
        grep -q "yarn run start" /tmp/yarn-startup.sh
        sh /tmp/yarn-startup.sh >/tmp/yarn-startup.log 2>&1 &
        server_pid=$!
        trap "kill $server_pid 2>/dev/null || true" EXIT
        for attempt in $(seq 1 30); do
            if test "$(curl --silent --fail http://127.0.0.1:8124)" = "ok"; then
                break
            fi
            if ! kill -0 "$server_pid" 2>/dev/null; then
                cat /tmp/yarn-startup.log >&2
                exit 1
            fi
            sleep 1
        done
        test "$(curl --silent --fail http://127.0.0.1:8124)" = "ok"
        kill "$server_pid"
        wait "$server_pid" || true
        trap - EXIT

        mkdir -p /tmp/direct-app
        cp /tmp/npm-app/server.js /tmp/direct-app/index.js
        oryx create-script -appPath /tmp/direct-app -output /tmp/direct-startup.sh
        grep -q "node index.js" /tmp/direct-startup.sh

        mkdir -p /tmp/archive-source
        printf ok > /tmp/archive-source/sentinel
        for archive_type in tar.gz tar.zst zip; do
            mkdir -p "/tmp/archive-$archive_type"
            cat > "/tmp/archive-$archive_type/oryx-manifest.toml" <<EOF
CompressedNodeModulesFile = "node_modules.$archive_type"
EOF
            oryx create-script \
                -appPath "/tmp/archive-$archive_type" \
                -output "/tmp/archive-$archive_type/startup.sh" \
                -userStartupCommand "test \"\$(cat node_modules/sentinel)\" = ok"
        done
        tar -czf /tmp/archive-tar.gz/node_modules.tar.gz -C /tmp/archive-source .
        tar -I zstd -cf /tmp/archive-tar.zst/node_modules.tar.zst -C /tmp/archive-source .
        printf %s \
            UEsDBBQAAAAIAGOCKF1H3dx5BAAAAAIAAAAIAAAAc2VudGluZWzLzwYAUEsBAhQAFAAAAAgAY4IoXUfd3HkEAAAAAgAAAAgAAAAAAAAAAAAAAAAAAAAAAHNlbnRpbmVsUEsFBgAAAAABAAEANgAAACoAAAAAAA== \
            | base64 -d > /tmp/archive-zip/node_modules.zip
        grep -q "tar -xzf node_modules.tar.gz" /tmp/archive-tar.gz/startup.sh
        grep -q "tar -I zstd -xf node_modules.tar.zst" /tmp/archive-tar.zst/startup.sh
        grep -q "unzip -q node_modules.zip" /tmp/archive-zip/startup.sh
        sh /tmp/archive-tar.gz/startup.sh
        sh /tmp/archive-tar.zst/startup.sh
        sh /tmp/archive-zip/startup.sh

        EXPECTED_MAJOR="$EXPECTED_MAJOR" bash -c '"'"'
            source /opt/oryx/benv "node=$EXPECTED_MAJOR"
            test "$node" = "/opt/nodejs/$EXPECTED_MAJOR/bin/node"
            test "$("$node" -p "process.versions.node")" = "$NODE_VERSION"
        '"'"'
    '

    docker build \
        --file "$NATIVE_MODULE_TEST_DOCKERFILE" \
        --build-arg "NODE_MAJOR=$expected_major" \
        --build-arg "RUNTIME_IMAGE=$image" \
        --output type=cacheonly \
        "$REPO_DIR"
done < "$1"

echo "All Embr Node runtime image tests passed."
