#!/bin/bash

set -e

# Wait for the app source to be uploaded to the blob storage.
formatted_date=$(date -u +"%a, %d %b %Y %H:%M:%S GMT")
auth_header="Authorization: Bearer $MI_ACCESS_TOKEN"
version_header="x-ms-version: 2019-12-12"
date_header="x-ms-date: $formatted_date"
file_upload_endpoint="$FILE_UPLOAD_CONTAINER_URL/$FILE_UPLOAD_BLOB_NAME"

temp_app_source_dir="/tmp/appsource"
temp_app_source_path="$temp_app_source_dir/$FILE_UPLOAD_BLOB_NAME"
mkdir $temp_app_source_dir

# Send request to delete the blob from the storage account on exit.
# trap 'curl -H '$headers' -X GET "'$file_upload_endpoint'"' EXIT

while [[ ! -f "$temp_app_source_path" || ! "$(file $temp_app_source_path)" =~ "compressed data" ]]
do
  echo "Waiting for app source to be uploaded. Please upload the app source to the endpoint specified in the Build resource's 'uploadEndpoint' property."
  curl -H "$auth_header" -H "$version_header" -H "$date_header" -X GET "$file_upload_endpoint" -o "$temp_app_source_path" -s
  sleep 5
done

# Extract app code to CNB_APP_DIR directory.
echo "Found app source at '$temp_app_source_path'. Extracting to $CNB_APP_DIR"
mkdir -p $CNB_APP_DIR
cd $CNB_APP_DIR
tar -xzf "$temp_app_source_path"

fileCount=$(ls | wc -l)
if [ "$fileCount" = "1" ]; then
  # Find .jar file in the directory
  jarfile=$(find "$CNB_APP_DIR" -maxdepth 1 -name "*.jar" | head -n 1)

  # unzip it if found
  if [[ -n $jarfile ]];
  then 
    echo "Unzip jar file $jarfile"
    unzip -qq $jarfile -d $CNB_APP_DIR
    rm $jarfile
  fi
fi

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