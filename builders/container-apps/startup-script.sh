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
temp_app_header_path="$temp_app_source_dir/header.txt"
mkdir $temp_app_source_dir

# List all the environment variables and filter the environment variable with prefix "ACA_CLOUD_BUILD_USER_ENV_", then write them to folder "/platform/env", 
# file name is the environment variable name without prefix, file content is the environment variable value.
build_env_dir="/platform/env"
env | grep -E '^ACA_CLOUD_BUILD_USER_ENV_' | while read -r line; do
  key=$(echo "$line" | cut -d= -f1)
  value=$(echo "$line" | cut -d= -f2-)
  filename="${key#ACA_CLOUD_BUILD_USER_ENV_}"
  echo -n "$value" > "$build_env_dir/$filename"
done

# write environment variable CORRELATION_ID to folder "/platform/env", 
if [ -n "$CORRELATION_ID" ]; then
  echo -n "$CORRELATION_ID" > "$build_env_dir/CORRELATION_ID"
fi

file_extension=""
while [[ ! -f "$temp_app_source_path" || ! "$(file $temp_app_source_path)" =~ "compressed data" ]]
do
  echo "Waiting for app source to be uploaded. Please upload the app source to the endpoint specified in the Build resource's 'uploadEndpoint' property."
  curl -H "$auth_header" -H "$version_header" -H "$date_header" -X GET "$file_upload_endpoint" -o "$temp_app_source_path" -D "$temp_app_header_path" -s
  if [[ -f "$temp_app_header_path" ]]; then
    file_extension=$(grep -i x-ms-meta-FileExtension "$temp_app_header_path" | cut -d ' ' -f2)
    # Check if the original file extension is .jar, .war, .zip or .tar.gz
    if [[ "$file_extension" =~ ".tar.gz" 
          || "$file_extension" =~ ".jar" 
          || "$file_extension" =~ ".war" 
          || "$file_extension" =~ ".zip"  ]]; then
      break
    fi
  fi
  sleep 5
done

# Extract app code to CNB_APP_DIR directory.
echo "Found app source at '$temp_app_source_path'. Extracting to $CNB_APP_DIR"
mkdir -p $CNB_APP_DIR
cd $CNB_APP_DIR

if [[ "$file_extension" =~ ".jar" 
      || "$file_extension" =~ ".war" 
      || "$file_extension" =~ ".zip"  ]]; then
  unzip -qq "$temp_app_source_path"
else
# Keep compatibility with old logic
  tar -xzf "$temp_app_source_path"
fi

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

RETRY_DELAY=5
RETRY_ATTEMPTS=6

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