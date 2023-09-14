#!/bin/bash

set -e

# Wait for tarball to be placed in COMPRESSED_APP_LOCATION, and the type to be compressed data.
# Waiting for the type to be compressed data also helps ensure that the file is fully uploaded before
# proceeding.
while [[ ! -f $COMPRESSED_APP_LOCATION || ! "$(file $COMPRESSED_APP_LOCATION)" =~ "compressed data" ]]
do
  echo "Waiting for app source to be uploaded to '$COMPRESSED_APP_LOCATION'..."
  sleep 5
done

# Extract app code to CNB_APP_DIR directory.
echo "Found app source at '$COMPRESSED_APP_LOCATION'. Extracting to $CNB_APP_DIR"
mkdir -p $CNB_APP_DIR
cd $CNB_APP_DIR
tar -xzf "$COMPRESSED_APP_LOCATION"

# public cert should be in this env var
ca_pem_decoded=$(printf "%s" "$REGISTRY_HTTP_TLS_CERTIFICATE" | base64 -d)
echo "$ca_pem_decoded" >> /usr/local/share/ca-certificates/internalregistry.crt
cd /usr/local/share/ca-certificates/
awk 'BEGIN {c=0;} /BEGIN CERT/{c++} { print > "cert." c ".crt"}' < /usr/local/share/ca-certificates/internalregistry.crt
update-ca-certificates

cd $CNB_APP_DIR
token=$(printf "%s" "$REGISTRY_AUTH_USERNAME:$REGISTRY_AUTH_PASSWORD" | base64)
acr_access_string="Basic $token"
export CNB_REGISTRY_AUTH='{"'$ACR_RESOURCE_NAME'":"'$acr_access_string'"}'

# Execute the analyze phase
echo
echo "======================================="
echo "===== Executing the analyze phase ====="
echo "======================================="
/lifecycle/analyzer \
  -log-level debug \
  -run-image cormtestacr.azurecr.io/oryx/builder:stack-run-debian-bullseye-20230817.1 \
  $APP_IMAGE

# Execute the detect phase
echo
echo "======================================"
echo "===== Executing the detect phase ====="
echo "======================================"
/lifecycle/detector \
  -log-level debug \
  -app $CNB_APP_DIR

# Execute the restore phase
echo
echo "======================================="
echo "===== Executing the restore phase ====="
echo "======================================="
/lifecycle/restorer \
  -log-level debug \
  -cache-dir /cache \
  -build-image cormtestacr.azurecr.io/oryx/builder:stack-build-debian-bullseye-20230817.1

# Execute the extend phase
echo
echo "======================================"
echo "===== Executing the extend phase ====="
echo "======================================"
/lifecycle/extender \
  -log-level debug \
  -app $CNB_APP_DIR

# Execute the export phase
echo
echo "======================================"
echo "===== Executing the export phase ====="
echo "======================================"
/lifecycle/exporter \
  -log-level debug \
  -cache-dir /cache \
  -app $CNB_APP_DIR \
  $APP_IMAGE