#!/usr/bin/env bash

# Purpose: Import Azure Linux CA bundle certificates into Debian/Ubuntu trust store
# Behavior: Safely splits bundle, validates, deduplicates (system + within batch), installs, refreshes store.

set -euxo pipefail

readonly TMP_CERTS_DIR="${1:-}"
readonly BUNDLE_PATH="${2:-}"

usage() {
    echo "Usage: $0 <temp_work_dir> <bundle_path>" >&2
}
readonly DEST_DIR="/usr/local/share/ca-certificates"

cleanup() {
    # Only remove directory if it looks like a mktemp-style path or explicitly provided and still exists
    if [ -n "${TMP_CERTS_DIR}" ] && [ -d "${TMP_CERTS_DIR}" ]; then
        rm -rf "${TMP_CERTS_DIR}"
    fi
}

# Ensure cleanup on exit, including on interrupts
trap cleanup EXIT INT TERM

log() { printf '%s: %s\n' "$1" "${*:2}"; }
warn() { log "WARN" "$@" >&2; }
info() { log "INFO" "$@"; }
err()  { log "ERROR" "$@" >&2; }

require_tools() {
    for t in openssl update-ca-certificates; do
        if ! command -v "$t" >/dev/null 2>&1; then
            warn "Required tool '$t' is not installed."
            return 1
        fi
    done
    return 0
}

install_prereqs() {
    if require_tools; then
        return 0
    fi

    apt-get update && \
    apt-get install -y --no-install-recommends \
        openssl ca-certificates && \
    rm -rf /var/lib/apt/lists/*
}

split_bundle() {
    local bundle="$1" outdir="$2"
    awk 'BEGIN{n=0}
        /-----BEGIN CERTIFICATE-----/{file=sprintf("%s/cert-%05d.pem", dir, ++n)}
        /-----BEGIN CERTIFICATE-----/,/-----END CERTIFICATE-----/ {print >> file; if(/-----END CERTIFICATE-----/) close(file)}
    ' dir="$outdir" "$bundle" || return 1
}

sanitize_filename() {
    tr '[:space:]/,()' '_' | tr -cd 'A-Za-z0-9_.-' | tr -s '_.-' | sed -E 's/^[_.-]+//; s/[_.-]+$//' | cut -c1-60
}

prepare_certificate_metadata() {
    local metadata_dir="$1"
    shift
    mkdir -p "$metadata_dir" || return 1
    [ "$#" -gt 0 ] || return 0

    # Inspect independently; consume results in original order below.
    printf '%s\0' "$@" | xargs -0 -r -n16 -P4 bash -ec '
        metadata_dir="$1"
        shift
        for certificate; do
            output="$metadata_dir/${certificate##*/}"
            status=0
            openssl x509 -in "$certificate" -noout -subject -nameopt RFC2253 -fingerprint -sha256 \
                > "$output.txt" 2> "$output.stderr" || status=$?
            printf "%s\n" "$status" > "$output.status"
        done
    ' -- "$metadata_dir"
}

process_certificate() {
    local azlCert="$1" dest_dir="$2" idx="$3"
    local metadata fp="" subject="" line

    local metadata_path="$TMP_CERTS_DIR/bundle-metadata/${azlCert##*/}"
    if [ "$(< "$metadata_path.status")" != 0 ]; then
        err "Invalid cert: $azlCert"
        return 1
    fi
    metadata=$(< "$metadata_path.txt")
    while IFS= read -r line; do
        case "$line" in
            subject=*) subject="$line" ;;
            *Fingerprint=*) fp="${line#*=}" ;;
        esac
    done <<< "$metadata"

    [ -n "$fp" ] || { err "Could not compute fingerprint for $azlCert"; return 1; }

    # Duplicate in already installed store?
    if grep -Fxq "$fp" "$TMP_CERTS_DIR/system-fingerprints.txt"; then
        info "Skip duplicate (system)"
        return 1
    fi

    # Duplicate within this batch?
    if grep -Fxq "$fp" "$TMP_CERTS_DIR/imported-azl-fingerprints.txt"; then
        info "Skip duplicate (batch): $azlCert"
        return 1
    fi
    printf '%s\n' "$fp" >> "$TMP_CERTS_DIR/imported-azl-fingerprints.txt"

    # Derive name
    local cn
    cn=$(printf '%s' "$subject" | sed -n 's/.*CN=\([^,]*\).*/\1/p')

    # If CN extraction failed, fallback to full subject or fingerprint
    [ -z "$cn" ] && cn=$(printf '%s' "$subject" | sed 's/^subject=//')
    [ -z "$cn" ] && cn="$fp"

    local filename
    filename=$(printf '%s' "$cn" | sanitize_filename)
    [ -z "$filename" ] && filename="cert_${idx}"

    local dest="$dest_dir/azl_${filename}.crt"

    if [ -e "$dest" ]; then
        # warn because this is not expected, but we can continue processing other certs
        warn "Name collision, skipping: $dest"
        return 1
    fi

    install -m 644 "$azlCert" "$dest"
    info "Imported: $(basename "$dest")"
    return 0
}

extract_system_fingerprints() {
    local existing line metadata_path
    local -a certificates=()
    if [ -d "/etc/ssl/certs" ]; then
        for existing in /etc/ssl/certs/*.pem; do
            [ -f "$existing" ] || continue
            certificates+=("$existing")
        done
        if ! prepare_certificate_metadata "$TMP_CERTS_DIR/system-metadata" "${certificates[@]}"; then
            err "Failed to inspect system certificates."
            return 1
        fi
        for existing in "${certificates[@]}"; do
            metadata_path="$TMP_CERTS_DIR/system-metadata/${existing##*/}"
            if [ -s "$metadata_path.stderr" ]; then
                cat "$metadata_path.stderr" >&2
            fi
            [ "$(< "$metadata_path.status")" = 0 ] || continue
            while IFS= read -r line; do
                case "$line" in
                    *Fingerprint=*) printf '%s\n' "${line#*=}" >> "$TMP_CERTS_DIR/system-fingerprints.txt" ;;
                esac
            done < "$metadata_path.txt"
        done

        if [ -s "$TMP_CERTS_DIR/system-fingerprints.txt" ]; then
            sort -u "$TMP_CERTS_DIR/system-fingerprints.txt" -o "$TMP_CERTS_DIR/system-fingerprints.txt"
        fi
    fi
}

main() {
    if [ -z "$TMP_CERTS_DIR" ] || [ -z "$BUNDLE_PATH" ]; then
        usage
        return 1
    fi

    if [ ! -d "$TMP_CERTS_DIR" ]; then
        err "Temp directory does not exist: $TMP_CERTS_DIR."
        return 1
    fi

    if [ ! -f "$BUNDLE_PATH" ]; then
        err "Bundle not present: $BUNDLE_PATH."
        return 1
    fi

    if [ ! -s "$BUNDLE_PATH" ]; then
        err "Bundle empty: $BUNDLE_PATH. This is unexpected."
        return 1
    fi

    install_prereqs || true

    if ! require_tools; then
        err "Prerequisite tools not available after attempted install. Aborting."
        return 1
    fi

    mkdir -p "$DEST_DIR"

    # Precompute system certificate fingerprints once for fast lookup
    : > "$TMP_CERTS_DIR/system-fingerprints.txt"
    extract_system_fingerprints

    if ! split_bundle "$BUNDLE_PATH" "$TMP_CERTS_DIR"; then
        err "Failed to split certificate bundle at $BUNDLE_PATH"
        return 1
    fi

    local count=0 idx=0 pem
    local -a certificates=()
    : > "$TMP_CERTS_DIR/imported-azl-fingerprints.txt"
    for pem in "$TMP_CERTS_DIR"/cert-*.pem; do
        [ -f "$pem" ] || continue
        certificates+=("$pem")
    done
    if ! prepare_certificate_metadata "$TMP_CERTS_DIR/bundle-metadata" "${certificates[@]}"; then
        err "Failed to inspect bundle certificates."
        return 1
    fi

    for pem in "${certificates[@]}"; do
        idx=$((idx+1))

        if process_certificate "$pem" "$DEST_DIR" "$idx"; then
            count=$((count+1))
        fi
    done

    if [ "$count" -gt 0 ]; then
        if ! update-ca-certificates --fresh >/dev/null 2>&1 && ! update-ca-certificates >/dev/null 2>&1; then
            err "Failed to refresh CA certificates after importing Azure Linux CAs."
            return 1
        fi
        info "Imported $count Azure Linux certificate(s)."
    else
        info "No new certificates imported."
    fi
}

main "$@"
