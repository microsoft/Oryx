#!/bin/bash
autoAssign() {
  echo "Current oncall is $1"
  response=`curl -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $2" https://api.github.com/repos/microsoft/Oryx/issues | jq '.[]| select(.assignee == null or .assignee == "")' | jq '.number'`
    arr=($response)
    for i in "${arr[@]}";
    do
      curl -X POST "https://api.github.com/repos/microsoft/Oryx/issues/$i/assignees" -H "Accept: application/vnd.github+json" -H "Authorization: Bearer $2" -d "{\"assignees\": ["\"$1\""]}"
    done
}

file=$1
token=$2

if [ ! -f "$file" ]; then
  echo "Error: Custom shift file not found"
  exit 1
fi

today=$(date +%s)

while read line || [ -n "$line" ]; do
  if [ "${line:0:1}" = "#" ]
    then
      continue
  else
    # Extract the name, start date, and end date from the line
    name=$(echo $line | cut -d ' ' -f1)
    start_date=$(echo $line | cut -d ' ' -f 2)
    start_date=$(date -d $start_date +%s)
    end_date=$(echo $line | cut -d ' ' -f 3)
    end_date=$(date -d $end_date +%s)
    # Check if the current date is within the date range
    if [ "$today" -ge "$start_date" ] && [ "$today" -le "$end_date" ]; then
      echo "Custom shift found"
      autoAssign $name $token
      exit 0
    fi
  fi
done < "$file"

oncallList=("waliMSFT" "cormacpayne" "harryli0108" "snehapar9" "pauld-msft" "william-msft")
oncallLen=${#oncallList[@]}
# achor Date is Feb 21 2022 0:00 PST
anchorDate=1677002400
d=$(((today - anchorDate)/60/60/24/7))
pos=`echo "$d%$oncallLen" | bc`
currentOncall=`echo ${oncallList[$pos]}`

autoAssign $currentOncall $token
exit 0