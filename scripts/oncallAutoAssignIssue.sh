#!/bin/bash

autoIssue() {
  assigneeName=$1
  token=$2
  issueNum=$3
  # Check if the user can be assigned
  checkAccess=$(curl --silent --output /dev/null --write-out "%{http_code}" -L -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $token" -H "X-GitHub-Api-Version: 2022-11-28" https://api.github.com/repos/microsoft/Oryx/assignees/$assigneeName)
  if [[ "$checkAccess" =~ ^2 ]]; then
    res=$(curl --silent --output /dev/null --write-out "%{http_code}" -X POST "https://api.github.com/repos/microsoft/Oryx/issues/$issueNum/assignees" -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $token" -d "{\"assignees\": ["\"$assigneeName\""]}")
    # Check for curl response status
    if [[ "$res" =~ ^2 ]]; then
      return 0
    else
      echo "Error: Failed to assign isssue $issueNum. Check token and issue status."
      exit 1
    fi
  else
    echo "Error: Engineer Access denied."
    exit 1
  fi
}

tryAssignIssueToCustomOncall() {
  input=$1
  today=$2
  token=$3
  issue=$4
  if [ -z "$input" ]; then
    return 1
  fi
  # Custom oncall rotation defined in repo variable. Using format:
  # "{github-username},{start-date},{end-date}"
  # Double quote is required.
  IFS="," read -ra arr <<< "$1"
  name=${arr[0]}
  startDate=${arr[1]}
  endDate=${arr[2]}
  startDate=$(date -d $startDate +%s)
  endDate=$(date -d $endDate +%s)
  if [ "$today" -ge "$startDate" ] && [ "$today" -le "$endDate" ]; then
    echo "Custom oncall found"
    autoIssue $name $token $issue
    exit 0
  fi
}

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
ONCALLS=""
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
    --custom) # Custom shift flag
    customRotation=$2
    shift 2
    ;;
    --)
    shift
    break
    ;;
    *)
    ONCALLS="$ONCALLS $1"
    shift
    ;;
  esac
done
eval set -- "$ONCALLS"

today=$(date +%s)

oncallArrLen=${#ONCALLS[@]}
# anchor Date is Feb 21 2022 0:00
anchorDate=1677002400
d=$(((today - anchorDate)/60/60/24/7))
pos=`echo "$d%$oncallArrLen" | bc`
currentOncall=`echo ${ONCALLS[$pos]}`

# Check if custom rotation argument is present or not
if [[ -v customRotation ]]; then
  # If yes, try assign it to custom oncall engineer
  # Function exit 0 when successfully assigned it to custom oncall engineer
  tryAssignIssueToCustomOncall $customRotation $today $authToken $issueNum
fi
# If failed, then assign to current oncall.
autoIssue $currentOncall $authToken $issueNum
exit 0