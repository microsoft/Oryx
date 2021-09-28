#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
# The purpose of the file is share common functionality 
# across platforms for BuildScriptGenerator
# Example:
#   src/BuildScriptGenerator/Python/PythonBashBuildSnippet.sh.tpl
#   src/BuildScriptGenerator/Node/NodeBashBuildSnippet.sh.tpl

function LogError()
{
    if ["$#" - ne 2]; then
       echo "Please provide 2 paremter to LogError: "
       echo "LogError {scriptName} {errorMessage}"
    fi
    _LogMessage "ERROR" "$1" "$2"
}

function LogWarning()
{
    if ["$#" - ne 2]; then
       echo "Please provide 2 paremter to LogWarning: "
       echo "LogWarning {scriptName} {errorMessage}"
    fi
    _LogMessage "WARNING" "$1" "$2"
}

function _LogMessage()
{
# Logs:
# Timestamp|{Type}|{FileName}|{Message}
# Example:
#       2021-09-28 00:17:12|ERROR|FileName|Error Message
    timestamp =$(date + "%F %T"--date = 'TZ="US/Pacific"')
    messageType =$1
    scriptName =$2
    errorMessage =$3

    echo "${timestamp}|${messageType}|${scriptName}|${errorMessage}"
}