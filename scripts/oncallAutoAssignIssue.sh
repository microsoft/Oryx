#!/bin/bash

autoIssue() {
  assigneeName=$1
  token=$2
  issueNum=$3
  # Check if the user can be assigned
  echo "Attempting to assign user $assigneeName to issue $issueNum"
  checkAccess=$(curl --silent --output /dev/null --write-out "%{http_code}" -L -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $token" -H "X-GitHub-Api-Version: 2022-11-28" https://api.github.com/repos/microsoft/Oryx/assignees/$assigneeName)
  if [[ "$checkAccess" =~ ^2 ]]; then
    res=$(curl --silent --output /dev/null --write-out "%{http_code}" -X POST "https://api.github.com/repos/microsoft/Oryx/issues/$issueNum/assignees" -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $token" -d "{\"assignees\": ["\"$assigneeName\""]}")
    # Check for curl response status
    if [[ "$res" =~ ^2 ]]; then
      exit 0
    else
      echo "Error: Failed to assign isssue $issueNum. Check token and issue status."
      exit 1
    fi
  else
    echo "Error: Engineer Access denied."
    return 1
  fi
}

tryAssignIssueToCustomOncall() {
  today=$1
  token=$2
  issue=$3
  input="$CUSTOM_ONCALL_ROTATION"
  # Github parses new line as "\n\r". We're removing the "\n" and changing "\r" to ";" in order to parse entries
  # Removing "\n"
  input="${input//$'\n'/''}"
  # Replacing "\r" with ";"
  input="${input//$'\r'/';'}"
  if [ -z "$input" ]; then
    return 1
  fi
  # Custom oncall rotation defined in repo variable. Using format:
  # {github-username},{start-date},{end-date}

  # Divide each entry by ";"
  IFS=';' read -ra arr <<< "$input"
  for i in "${arr[@]}"
  do
    # Divide each field by ","
    IFS=',' read -ra entry <<< "$i"
    name=$(echo ${entry[0]})
    startDate=$(echo ${entry[1]})
    endDate=$(echo ${entry[2]})
    startDate=$(date -d $startDate +%s)
    endDate=$(date -d $endDate +%s)
    if [ "$today" -ge "$startDate" ] && [ "$today" -le "$endDate" ]; then
      echo "Custom oncall found for $name"
      autoIssue $name $token $issue
    fi
  done
}

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
  case "$1" in
    --token) #Github token flag
    authToken=$2
    shift 2
    ;;
    --issue) # Issue number flag
    issueNum=$2
    shift 2
    ;;
    --)
    shift
    break
    ;;
    *)
    PARAMS="$PARAMS $1"
    shift
    ;;
  esac
done
eval set -- "$PARAMS"

today=$(date +%s)

# Oncall rotation is defined in repo variable. Using format:
# {github-username-1},{github-username-2},...
# Parsing oncall engineer lists into array and seperating them by ','
IFS=',' read -ra parsedOncalls <<< "$ONCALL_LIST"

oncallArrLen=${#parsedOncalls[@]}
# anchor Date is Nov 7 2023 10:00 PST
anchorDate=1699380000
d=$(((today - anchorDate)/60/60/24/7))
pos=`echo "$d%$oncallArrLen" | bc`
currentOncall=`echo ${parsedOncalls[$pos]}`

# Check if custom rotation env var is present or not
if [ -n "$CUSTOM_ONCALL_ROTATION" ]; then
  # If yes, try assign it to custom oncall engineer
  # Function exit 0 when successfully assigned it to custom oncall engineer
  tryAssignIssueToCustomOncall $today $authToken $issueNum
fi
# If failed, then assign to current oncall.
autoIssue $currentOncall $authToken $issueNum
exit 1