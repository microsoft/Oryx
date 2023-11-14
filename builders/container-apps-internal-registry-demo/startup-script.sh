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

build_env_dir="/platform/env"
# list all the environment variables and filter the environment variable with prefix BP_, BPL_, ORYX_ or CORRELATION_ID, then write them to the build env dir
env | grep -E '^BP_|^BPL_|^ORYX_|^CORRELATION_ID$' | while read -r line; do
  var_name=$(echo "$line" | cut -d= -f1)
  var_value=$(echo "$line" | cut -d= -f2-)
  echo "$var_value" > "$build_env_dir/$var_name"
done

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
  # Find .jar/war file in the directory
  artifact=$(find "$CNB_APP_DIR" -maxdepth 1 -name "*.[jw]ar" | head -n 1)

  # unzip it if found
  if [[ -n $artifact ]];
  then
    echo "Unzip file $artifact"
    unzip -qq $artifact -d $CNB_APP_DIR
    rm $artifact
  fi
fi

# public cert should be in this env var
ca_pem_decoded=$(printf "%s" "$REGISTRY_HTTP_TLS_CERTIFICATE" | base64 -d)
echo "$ca_pem_decoded" > /usr/local/share/ca-certificates/internalregistry.crt
cd /usr/local/share/ca-certificates/
awk 'BEGIN {c=0;} /BEGIN CERT/{c++} { print > "cert." c ".crt"}' < /usr/local/share/ca-certificates/internalregistry.crt
update-ca-certificates

cd $CNB_APP_DIR
token=$(printf "%s" "$REGISTRY_AUTH_USERNAME:$REGISTRY_AUTH_PASSWORD" | base64)
acr_access_string="Basic $token"
export CNB_REGISTRY_AUTH='{"'$ACR_RESOURCE_NAME'":"'$acr_access_string'"}'

echo "----- Initiating buildpack build -----"
echo "----- Cloud Build correlation id: '$CORRELATION_ID' -----"
echo 

RETRY_DELAY=2
RETRY_ATTEMPTS=5

function fail_if_retry_exceeded() {
  retries=$1
  exitCode=$2
  if [ "$retries" -ge $RETRY_ATTEMPTS ]; then
    echo "----- Retry attempts exceeded -----"
    echo "----- Cloud Build failed with exit code '$lifecycleExitCode' -----"
    exit $exitCode
  fi
}

# Allow commands to fail, so we can parse exit codes and handle the failures ourselves.
set +e

# Execute the analyze phase
echo
echo "===== Executing the analyze phase ====="
retryCount=0
lifecycleExitCode=0
until [ "$retryCount" -ge $RETRY_ATTEMPTS ]
do
  if [ "$retryCount" -ge 1 ]; then
    echo "===== Retrying analyze phase (attempt $retryCount) ====="
  fi

  /lifecycle/analyzer \
    -log-level debug \
    -run-image mcr.microsoft.com/oryx/builder:stack-run-debian-bullseye-20230926.1 \
    $APP_IMAGE

  lifecycleExitCode=$?
  if [ "$lifecycleExitCode" -eq 0 ]; then
    break
  fi

  retryCount=$((retryCount+1))
  sleep $RETRY_DELAY
done

fail_if_retry_exceeded $retryCount $lifecycleExitCode

# Execute the detect phase
echo
echo "===== Executing the detect phase ====="
retryCount=0
lifecycleExitCode=0
until [ "$retryCount" -ge $RETRY_ATTEMPTS ]
do
  if [ "$retryCount" -ge 1 ]; then
    echo "===== Retrying detect phase (attempt $retryCount) ====="
  fi

  /lifecycle/detector \
    -log-level debug \
    -app $CNB_APP_DIR

  lifecycleExitCode=$?
  if [ "$lifecycleExitCode" -eq 0 ]; then
    break
  fi

  retryCount=$((retryCount+1))
  sleep $RETRY_DELAY
done

fail_if_retry_exceeded $retryCount $lifecycleExitCode

# Execute the restore phase
echo
echo "===== Executing the restore phase ====="
retryCount=0
lifecycleExitCode=0
until [ "$retryCount" -ge $RETRY_ATTEMPTS ]
do
  if [ "$retryCount" -ge 1 ]; then
    echo "===== Retrying restore phase (attempt $retryCount) ====="
  fi

  /lifecycle/restorer \
    -log-level debug \
    -build-image mcr.microsoft.com/oryx/builder:stack-build-debian-bullseye-20230926.1

  lifecycleExitCode=$?
  if [ "$lifecycleExitCode" -eq 0 ]; then
    break
  fi

  retryCount=$((retryCount+1))
  sleep $RETRY_DELAY
done

fail_if_retry_exceeded $retryCount $lifecycleExitCode

# Execute the extend phase
# Note: we do not retry this, as generally these failures are from the actual build rather than infrastructure.
echo
echo "===== Executing the extend phase ====="

/lifecycle/extender \
  -log-level debug \
  -app $CNB_APP_DIR

lifecycleExitCode=$?
if [ $lifecycleExitCode -ne 0 ]; then
    echo "----- Cloud Build failed with exit code '$lifecycleExitCode' -----"
    exit $lifecycleExitCode
fi

# Execute the export phase
echo
echo "===== Executing the export phase ====="
retryCount=0
lifecycleExitCode=0
until [ "$retryCount" -ge $RETRY_ATTEMPTS ]
do
  if [ "$retryCount" -ge 1 ]; then
    echo "===== Retrying export phase (attempt $retryCount) ====="
  fi

  /lifecycle/exporter \
    -log-level debug \
    -app $CNB_APP_DIR \
    $APP_IMAGE

  lifecycleExitCode=$?
  if [ "$lifecycleExitCode" -eq 0 ]; then
    break
  fi

  retryCount=$((retryCount+1))
  sleep $RETRY_DELAY
done

fail_if_retry_exceeded $retryCount $lifecycleExitCode