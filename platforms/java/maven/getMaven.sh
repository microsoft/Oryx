#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

mavenPlatformDir="$REPO_DIR/platforms/java/maven"
hostMavenArtifactsDir="$volumeHostDir/maven"
mkdir -p "$hostMavenArtifactsDir"

rm -rf /tmp/maven
mkdir -p /tmp/maven
cd /tmp/maven
baseUrl="http://www.gtlib.gatech.edu/pub/apache/maven/maven-3"

downloadMavenBinary()
{
    local version="$1"
    tarFileName="maven-$version.tar.gz"
    if shouldBuildSdk maven $tarFileName || shouldOverwriteSdk || shouldOverwriteMavenBinary; then
        curl -L "$baseUrl/$version/binaries/apache-maven-$version-bin.tar.gz" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        cd "extracted/apache-maven-$version"
        tar -zcf "$hostMavenArtifactsDir/$tarFileName" .

		echo "Version=$version" >> "$hostMavenArtifactsDir/maven-$version-metadata.txt"
    fi
}

shouldOverwriteMavenBinary() {
	if [ "$OVERWRITE_EXISTING_SDKS_MAVEN" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Downloading Maven binary..."
echo
buildPlatform "$mavenPlatformDir/versionsToBuild.txt" downloadMavenBinary

cp "$mavenPlatformDir/defaultVersion.txt" $hostMavenArtifactsDir

ls -l $hostMavenArtifactsDir

