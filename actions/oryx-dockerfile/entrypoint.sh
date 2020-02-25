#!/bin/sh -l

sourceDirectory=$1
platform=$2
platformVersion=$3
dockerfilePath="Dockerfile.oryx"


echo

if [ -n "${sourceDirectory}" ]
then
    dockerfilePath="$sourceDirectory/$dockerfilePath"
    sourceDirectory="$PWD/$sourceDirectory"
    echo "Relative path to source directory provided -- the following directory will be built: '${sourceDirectory}'"
else
    sourceDirectory=$PWD
    echo "No source directory provided -- the root of the repository ('GITHUB_WORKSPACE' environment variable) will be built: '${sourceDirectory}'"
fi

oryxCommand="oryx dockerfile ${sourceDirectory} --output ${dockerfilePath}"

echo
echo "Dockerfile will be written to the following file: '${dockerfilePath}'"
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
echo
eval $oryxCommand

if [ -f "$dockerfilePath" ];
then
    echo "Dockerfile generation succeeded; the following is the content of the Dockerfile:"
    cat $dockerfilePath
else
    echo "Dockerfile generation failed."
    exit 1
fi

echo
echo ::set-output name=dockerfile-path::$dockerfilePath