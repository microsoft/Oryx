#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

javaPlatformDir="$REPO_DIR/platforms/java"
hostJavaArtifactsDir="$volumeHostDir/java"
debianFlavor="$1"
sdkStorageAccountUrl="$2"
mkdir -p "$hostJavaArtifactsDir"

rm -rf /tmp/java
mkdir -p /tmp/java
cd /tmp/java

downloadJavaSdk()
{
    local JDK_VERSION="$1"
    local JDK_BUILD_NUMBER="$2"
    local JDK_SHA256="$3"
    local JDK_URL="$4"  
    local JDK_DIR_NAME="$5" # jdk's root directory name after extracting

    tarFileName="java-$JDK_VERSION.tar.gz"
    tarFileNameWithoutGZ="java-$JDK_VERSION.tar"
    metadataFile=""
    sdkVersionMetadataName=""

    # set tarFile and metadata's Debian flavor
    if [ "$debianFlavor" == "stretch" ]; then
            tarFileName=java-$JDK_VERSION.tar.gz
            tarFileNameWithoutGZ=java-$JDK_VERSION.tar
            metadataFile="$hostJavaArtifactsDir/java-$JDK_VERSION-metadata.txt"
            # Continue adding the version metadata with the name of Version
            # which is what our legacy CLI will use
            sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
            cp "$javaPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$hostJavaArtifactsDir/defaultVersion.txt"
    else
            tarFileName=java-$debianFlavor-$JDK_VERSION.tar.gz
            tarFileNameWithoutGZ=java-$debianFlavor-$JDK_VERSION.tar
            metadataFile="$hostJavaArtifactsDir/java-$debianFlavor-$JDK_VERSION-metadata.txt"
            sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
    fi

    
    if [ ! -z "$JDK_URL" ]; then
        # download & validate
        echo "JDK_URL: ${JDK_URL}"
        curl -L "${JDK_URL}" -o $tarFileName
        echo "$JDK_SHA256 $tarFileName" | sha256sum --check --strict; \
        
        # extract
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        ls
        if [ ! -z "$JDK_DIR_NAME" ]; then
            jdk_root="extracted/$JDK_DIR_NAME"
        else
            jdk_root="extracted/jdk-${JDK_VERSION}"
        fi
        cd $jdk_root
        tar -zcf "$hostJavaArtifactsDir/$tarFileName" .

        echo "$sdkVersionMetadataName=$JDK_VERSION" >> $metadataFile
        echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
        return
    fi

    # TODO: refactor to reduce the number of if statements. Workitem #1439235
    # Version 8 or 1.8.0 has a different url format than rest of the versions, so special casing it.
    if [ "$JDK_VERSION" == "1.8.0" ]; then
        local versionUpdate="8u265"
        local buildNumber="b01"
        local url="https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk${versionUpdate}-${buildNumber}/OpenJDK8U-jdk_x64_linux_hotspot_${versionUpdate}${buildNumber}.tar.gz"

        curl -L "$url" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        cd "extracted/jdk${versionUpdate}-${buildNumber}"
        tar -zcf "$hostJavaArtifactsDir/$tarFileName" .
        echo "$sdkVersionMetadataName=$JDK_VERSION" >> $metadataFile
        echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
        return
    fi

    IFS='.' read -ra VERSION_PARTS <<< "$JDK_VERSION"
    majorVersion="${VERSION_PARTS[0]}"

    if [ "$JDK_VERSION" == "17.0.2" ]; then
        local url="https://download.java.net/java/GA/jdk17.0.2/dfd4a8d0985749f896bed50d7138ee7f/8/GPL/openjdk-17.0.2_linux-x64_bin.tar.gz"

        curl -L "$url" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        gzip -d $tarFileName
        tar -xf $tarFileNameWithoutGZ --directory extracted
        cd "extracted/jdk-${JDK_VERSION}"
        tar -zcf "$hostJavaArtifactsDir/$tarFileName" .
        echo "$sdkVersionMetadataName=$JDK_VERSION" >> $metadataFile
        echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
        return
    fi

    if shouldBuildSdk java $tarFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk java; then
        local baseUrl="https://github.com/AdoptOpenJDK/openjdk${majorVersion}-binaries/releases/download"
        if [ "$majorVersion" == "10" ] && [ "$JDK_BUILD_NUMBER" == "13" ]; then
            url="$baseUrl/jdk-10.0.2%2B13.1/OpenJDK10U-jdk_x64_linux_hotspot_10.0.2_13.tar.gz"
        else
            url="$baseUrl/jdk-${JDK_VERSION}%2B${JDK_BUILD_NUMBER}/OpenJDK${majorVersion}U-jdk_x64_linux_hotspot_${JDK_VERSION}_${JDK_BUILD_NUMBER}.tar.gz"
        fi

        curl -L $url -o $tarFileName

        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        cd "extracted/jdk-${JDK_VERSION}+${JDK_BUILD_NUMBER}"
        tar -zcf "$hostJavaArtifactsDir/$tarFileName" .

        echo "$sdkVersionMetadataName=$JDK_VERSION" >> $metadataFile
        echo "JdkFullVersion=$JDK_VERSION+$JDK_BUILD_NUMBER" >> $metadataFile
        echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
    fi
}

echo "Downloading Java SDK..."
echo
buildPlatform "$javaPlatformDir/versions/$debianFlavor/versionsToBuild.txt" downloadJavaSdk

cp "$javaPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$hostJavaArtifactsDir/defaultVersion.$debianFlavor.txt"

ls -l $hostJavaArtifactsDir

