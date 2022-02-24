#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

function LogError()
{
    if [ $# -ne 1 ]; then
       echo "Please provide 1 paremter to LogError. Example: "
       echo "LogError {errorMessage}"
    fi
    _LogMessage "ERROR" "$1"
}

function LogWarning()
{
    if [ $# -ne 1 ]; then
       echo "Please provide 1 paremter to LogWarning. Example: "
       echo "LogWarning {errorMessage}"
    fi
    _LogMessage "WARNING" "$1"
}

function _LogMessage()
{
	# Logs:
	# Timestamp|{Type}|{Message}
	# Example:
	#       2021-09-28 00:17:12|ERROR|Error Message
    timestamp=$(date '+"%F %T"')  # UTC time-zone
    messageType="$1"
    errorMessage="$2"

    echo "${timestamp}|${messageType}|${errorMessage}"
}