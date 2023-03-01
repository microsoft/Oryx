#!/bin/bash

autoAssign() {
  curl -X POST "https://api.github.com/repos/microsoft/Oryx/issues/$3/assignees" -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $2" -d "{\"assignees\": ["\"$1\""]}"
}

checkCustomOncall() {
  input=$1
  today=$2
  token=$3
  issue=$4
  currentOncall=$5
  if [ -z "$input" ]; then
    return 1
  fi

  IFS="," read -ra arr <<< "$1"
  name=${arr[0]}
  startDate=${arr[1]}
  endDate=${arr[2]}
  startDate=$(date -d $startDate +%s)
  endDate=$(date -d $endDate +%s)
  if [ "$today" -ge "$startDate" ] && [ "$today" -le "$endDate" ]; then
    echo "Custom shift found"
    autoAssign $name $token $issue
    return 0
  else
    autoAssign $currentOncall $token $issue
    return 0
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
# achor Date is Feb 21 2022 0:00
anchorDate=1677002400
d=$(((today - anchorDate)/60/60/24/7))
pos=`echo "$d%$oncallArrLen" | bc`
currentOncall=`echo ${ONCALLS[$pos]}`

checkCustomOncall $customRotation $today $authToken $issueNum $currentOncall
exit 0