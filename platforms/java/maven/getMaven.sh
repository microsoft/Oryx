#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

mavenPlatformDir="$REPO_DIR/platforms/java/maven"
hostMavenArtifactsDir="$volumeHostDir/maven"
debianFlavor="$1"
mkdir -p "$hostMavenArtifactsDir"

rm -rf /tmp/maven
mkdir -p /tmp/maven
cd /tmp/maven
baseUrl="http://www.gtlib.gatech.edu/pub/apache/maven/maven-3"

downloadMavenBinary()
{
    local version="$1"
    tarFileName="maven-$version.tar.gz"

    if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			tarFileName=maven-$version.tar.gz
	else
			tarFileName=maven-$debianFlavor-$version.tar.gz
	fi

    if shouldBuildSdk maven $tarFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk maven; then
        curl -L "$baseUrl/$version/binaries/apache-maven-$version-bin.tar.gz" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        cd "extracted/apache-maven-$version"
        tar -zcf "$hostMavenArtifactsDir/$tarFileName" .
        
		echo "Version=$version" >> "$hostMavenArtifactsDir/maven-$version-metadata.txt"
    fi
}

echo "Downloading Maven binary..."
echo
buildPlatform "$mavenPlatformDir/maven/versions/$debianFlavor/versionsToBuild.txt" downloadMavenBinary

cp "$mavenPlatformDir/maven/versions/$debianFlavor/defaultVersion.txt" "$hostMavenArtifactsDir/defaultVersion.$debianFlavor.txt"

ls -l $hostMavenArtifactsDir

