#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# this method will execute a command
# and sleep & retry if there's a failure
# $1 
#	parameter contains the full command to be executed
retryCount=0
retries=5
while [ "$retryCount" -le "$retries" ]
do
	echo "retry $retryCount"
	$1 && break
	echo "error executing: $1"
	retryCount=$((retryCount+1)) 
	sleep 15
done
