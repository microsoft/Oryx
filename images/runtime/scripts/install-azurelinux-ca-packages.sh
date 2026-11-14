#!/bin/sh
set -eu

retry_tdnf() {
    for attempt in 1 2 3; do
        if tdnf "$@"; then
            return 0
        else
            status=$?
        fi
        # tdnf connection error 1208 is returned as shell exit status 184.
        if [ "$status" -ne 184 ] || [ "$attempt" -eq 3 ]; then
            return "$status"
        fi
        delay=$((attempt * 2))
        printf 'tdnf %s failed to connect (attempt %s/3); retrying in %s seconds.\n' \
            "$*" "$attempt" "$delay" >&2
        sleep "$delay"
    done
}

retry_tdnf makecache
retry_tdnf install -y ca-certificates
update-ca-trust extract
tdnf clean all
