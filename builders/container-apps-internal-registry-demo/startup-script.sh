#!/bin/bash

set -e

# Wait for tarball to be placed in COMPRESSED_APP_LOCATION, and the type to be compressed data.
# Waiting for the type to be compressed data also helps ensure that the file is fully uploaded before
# proceeding.
ls $COMPRESSED_APP_LOCATION 
echo "$(file $COMPRESSED_APP_LOCATION)"
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
ca_pem_decoded=$(printf "%s" "$CA_PEM" | base64 -d)
echo "$ca_pem_decoded" >> /usr/local/share/ca-certificates/internalregistry.crt
update-ca-certificates

# Detecting runtime stack and setting correct oryx runtime image.
runtime_stack=$(oryx dockerfile . | head -n 1 | sed 's/ARG RUNTIME=//')
export CNB_RUN_IMAGE="mcr.microsoft.com/oryx/$runtime_stack"

# hardcoded username / pwd for demo purposes
username="foo"
password="bar"
token=$(printf "%s" "$username:$password" | base64)
acr_access_string="Basic $token"
export CNB_REGISTRY_AUTH='{"'$ACR_RESOURCE_NAME'":"'$acr_access_string'"}'

# Execute the buildpack build using the /cnb/lifecycle/creator command.
# https://github.com/buildpacks/spec/blob/main/platform.md#creator
exec /lifecycle/creator --log-level=debug $APP_IMAGE