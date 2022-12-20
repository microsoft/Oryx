# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
# This script will check the pipeline status' result
# for a given a pipeline invocation id.
# A status.json file contains the meta data for a pipeline invocation id
# of the point in time it was called, until the result has 'succeeded'.
# The 'buildNumber' field of status.json is used later in the workflow
# to update constants.yaml runtime image tag.

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
	# store response in status.json for later use
	az pipelines runs show --id ${pipelineInvocationId} --organization https://devdiv.visualstudio.com/ --project DevDiv > /tmp/status.json
	result=$( cat /tmp/status.json | jq ".result" | tr -d '"' )
	echo "pipeline ${pipelineInvocationId} invocation result: $result"
	if [[ "$result" == "succeeded" ]]; then
		exit 0
	fi
	echo "retrying in $timeoutSeconds seconds..."
	retryCount=$((retryCount+1)) 
	sleep ${timeoutSeconds}
done

echo "The pipeline invocation has not succeeded within the allocated retries."
exit 1