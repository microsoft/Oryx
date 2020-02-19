#!/bin/sh -l

sourceDirectory=$1
platform=$2
platformVersion=$3

echo

if [ -n "${sourceDirectory}" ]
then
    sourceDirectory="$PWD/$sourceDirectory"
    echo "Relative path to source directory provided -- the following directory will be built: '${sourceDirectory}'"
else
    sourceDirectory=$PWD
    echo "No source directory provided -- the root of the repository ('GITHUB_WORKSPACE' environment variable) will be built: '${sourceDirectory}'"
fi

echo
oryxCommand="oryx build ${sourceDirectory}"
echo

if [ -n "${platform}" ]
then
    echo "Platform provided: '${platform}'"
    oryxCommand="${oryxCommand} --platform ${platform}"
else
    echo "No platform provided -- Oryx will enumerate the source directory to determine the platform."
fi

echo

if [ -n "${platformVersion}" ]
then
    echo "Platform version provided: '${platformVersion}'"
    oryxCommand="${oryxCommand} --platform-version ${platformVersion}"
else
    echo "No platform version provided -- Oryx will determine the version."
fi

echo
echo "Running command '${oryxCommand}'"

echo "GITHUB_RUN_ID is ${GITHUB_RUN_ID}."
echo "GITHUB_REPOSITORY is ${GITHUB_REPOSITORY}."

url="https://api.github.com/repos/${GITHUB_REPOSITORY}/actions/runs/${GITHUB_RUN_ID}/jobs"

#Gets the started time and completed time for building container within a Github Action.
#Format: "2020-02-15T02:51:50.000Z"
startTime=$(curl -X GET "${url}" | sed 's/,/\n/g' | grep "started_at" | awk '{print $2}' | sed -n '3p')
endTime=$(curl -X GET "${url}" | sed 's/,/\n/g' | grep "completed_at" | awk '{print $2}' | sed -n '3p')

echo "Started time is ${startTime}."
echo "Completed time is ${endTime}."

export GITHUB_BUILD_CONTAINER_START=$startTime
export GITHUB_BUILD_CONTAINER_COMPLETE=$endTime

eval $oryxCommand
