#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# this method will execute a command
# and sleep & retry if there's a failure
# $1 
#	parameter contains the full command to be executed
maxRetries=5
if [[ -n "${MAX_RETRIES}" ]]; then
  maxRetries=${MAX_RETRIES}
fi

timeoutSeconds=15
if [[ -n "${TIMEOUT_SECONDS}" ]]; then
  timeoutSeconds=${TIMEOUT_SECONDS}
fi

retryCount=0
while [ "$retryCount" -le "$maxRetries" ]
do
	echo "retry $retryCount"
	$1 && break
	echo "Failed command: $1"
	retryCount=$((retryCount+1)) 
	sleep ${timeoutSeconds}
done
