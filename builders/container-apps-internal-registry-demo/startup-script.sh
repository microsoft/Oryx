#!/bin/bash

set -e

# Wait for the app source to be uploaded to the blob storage.
headers = '{"Authorization":"Bearer '$MI_ACCESS_TOKEN'", "x-ms-version":"2019-02-02", "x-ms-date":"'$DATE'"}'
file_upload_endpoint = "$FILE_UPLOAD_CONTAINER_URL/$FILE_UPLOAD_BLOB_NAME"

# Send request to delete the blob from the storage account on exit.
trap 'curl -H '$headers' -X GET "'$file_upload_endpoint'"' EXIT

while [[ ! -f $FILE_UPLOAD_BLOB_NAME || ! "$(file $FILE_UPLOAD_BLOB_NAME)" =~ "compressed data" ]]
do
  echo "Waiting for app source to be uploaded. Please upload the app source to the endpoint specified in the Build resource's 'uploadEndpoint' property."
  curl -H $headers -X GET "$file_upload_endpoint" -o $FILE_UPLOAD_BLOB_NAME
  sleep 10
done

# Extract app code to CNB_APP_DIR directory.
echo "Found app source at '$FILE_UPLOAD_BLOB_NAME'. Extracting to $CNB_APP_DIR"
mkdir -p $CNB_APP_DIR
cd $CNB_APP_DIR
tar -xzf "$FILE_UPLOAD_BLOB_NAME"

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