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

# Detecting runtime stack and setting correct oryx runtime image.
runtime_stack=$(oryx dockerfile . | head -n 1 | sed 's/ARG RUNTIME=//')
export CNB_RUN_IMAGE="mcr.microsoft.com/oryx/$runtime_stack"

# Retrieving the acr access token for Bearer authentication per:
# https://github.com/Azure/acr/blob/main/docs/Token-BasicAuth.md#calling-an-azure-container-registry-api
# https://github.com/Azure/acr/blob/main/docs/AAD-OAuth.md#getting-credentials-programmatically
aad_access_token=$(curl -H "X-IDENTITY-HEADER: $IDENTITY_HEADER" \
    "$IDENTITY_ENDPOINT?resource=$MANAGEMENT_RESOURCE_URI&principal_id=$MI_PRINCIPAL_ID&api-version=2019-08-01" \
    | jq -r '.access_token')

acr_refresh_token=$(curl -X POST -H "Content-Type: application/x-www-form-urlencoded" -d \
    "grant_type=access_token&service=$ACR_RESOURCE_NAME&tenant=$TENANT_ID&access_token=$aad_access_token" \
    https://$ACR_RESOURCE_NAME/oauth2/exchange \
    | jq -r '.refresh_token')

acr_access_token=$(curl -X POST -H "Content-Type: application/x-www-form-urlencoded" -d \
    "grant_type=refresh_token&service=$ACR_RESOURCE_NAME&scope=$ACR_SCOPE&refresh_token=$acr_refresh_token" \
    https://$ACR_RESOURCE_NAME/oauth2/token \
    | jq -r '.access_token')

export CNB_REGISTRY_AUTH='{"'$ACR_RESOURCE_NAME'":"Bearer '$acr_access_token'"}'

# Execute the buildpack build using the /cnb/lifecycle/creator command.
# https://github.com/buildpacks/spec/blob/main/platform.md#creator
exec /lifecycle/creator --log-level=debug $APP_IMAGE