# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# this method will execute a command
# and sleep & retry if there's a failure
# $1 
#	parameter contains the full command to be executed

pipelineInvocationId=$1

maxRetries=3
if [[ -n "${MAX_RETRIES}" ]]; then
  maxRetries=${MAX_RETRIES}
fi

timeoutSeconds=10
if [[ -n "${TIMEOUT_SECONDS}" ]]; then
  timeoutSeconds=${TIMEOUT_SECONDS}
fi

retryCount=0
while [ "$retryCount" -le "$maxRetries" ]
do
	echo "retry $retryCount"
	# status.json contains the buildNumber, used later in the workflow
	date
	az pipelines runs show --id ${pipelineInvocationId} --organization https://devdiv.visualstudio.com/ --project DevDiv > status.json
	result=$( cat status.json | jq ".result" | tr -d '"' )
	echo "result: $result"
	if [[ "$result" == "succeeded" ]]; then
		exit 0
	fi
	echo "retrying in $timeoutSeconds seconds..."
	retryCount=$((retryCount+1)) 
	sleep ${timeoutSeconds}
done
