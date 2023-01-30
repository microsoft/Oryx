#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

mavenPlatformDir="$REPO_DIR/platforms/java/maven"
hostMavenArtifactsDir="$volumeHostDir/maven"
debianFlavor="$1"
sdkStorageAccountUrl="$2"
mkdir -p "$hostMavenArtifactsDir"

rm -rf /tmp/maven
mkdir -p /tmp/maven
cd /tmp/maven
baseUrl="https://archive.apache.org/dist/maven/maven-3"

downloadMavenBinary()
{
    local version="$1"
    tarFileName="maven-$version.tar.gz"
    metadataFile=""
    sdkVersionMetadataName=""

    if [ "$debianFlavor" == "stretch" ]; then
            # Use default sdk file name
            tarFileName=maven-$version.tar.gz
            metadataFile="$hostMavenArtifactsDir/maven-$version-metadata.txt"
            # Continue adding the version metadata with the name of Version
            # which is what our legacy CLI will use
            sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
            cp "$mavenPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$hostMavenArtifactsDir/defaultVersion.txt"
    else
            tarFileName=maven-$debianFlavor-$version.tar.gz
            metadataFile="$hostMavenArtifactsDir/maven-$debianFlavor-$version-metadata.txt"
            sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
    fi

    if shouldBuildSdk maven $tarFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk maven; then
        curl -L "$baseUrl/$version/binaries/apache-maven-$version-bin.tar.gz" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        cd "extracted/apache-maven-$version"
        tar -zcf "$hostMavenArtifactsDir/$tarFileName" .

        echo "$sdkVersionMetadataName=$version" >> $metadataFile
        echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
    fi
}

echo "Downloading Maven binary..."
echo
buildPlatform "$mavenPlatformDir/versions/$debianFlavor/versionsToBuild.txt" downloadMavenBinary

cp "$mavenPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$hostMavenArtifactsDir/defaultVersion.$debianFlavor.txt"

ls -l $hostMavenArtifactsDir

