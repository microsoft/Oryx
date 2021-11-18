#!/bin/bash
set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/platforms/__common.sh

javaPlatformDir="$REPO_DIR/platforms/java"
hostJavaArtifactsDir="$volumeHostDir/java"
debianFlavor="$1"
mkdir -p "$hostJavaArtifactsDir"

rm -rf /tmp/java
mkdir -p /tmp/java
cd /tmp/java

downloadJavaSdk()
{
    local JDK_VERSION="$1"
    local JDK_BUILD_NUMBER="$2"

    tarFileName="java-$JDK_VERSION.tar.gz"
    
    if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			tarFileName=java-$JDK_VERSION.tar.gz
	else
			tarFileName=java-$debianFlavor-$JDK_VERSION.tar.gz
	fi

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
		echo "Version=$JDK_VERSION" >> "$hostJavaArtifactsDir/java-$JDK_VERSION-metadata.txt"
        return
    fi

    IFS='.' read -ra VERSION_PARTS <<< "$JDK_VERSION"
    majorVersion="${VERSION_PARTS[0]}"

    # Version 17 has a different url format than rest of the versions, so special casing it.
    if [ "$majorVersion" == "17" ]; then
        local buildNumber="12"
        local url="https://download.java.net/java/GA/jdk${JDK_VERSION}/2a2082e5a09d4267845be086888add4f/${buildNumber}/GPL/openjdk-${JDK_VERSION}_linux-x64_bin.tar.gz"

        curl -L "$url" -o $tarFileName
        rm -rf extracted
        mkdir -p extracted
        tar -xf $tarFileName --directory extracted
        tar -zcf "$hostJavaArtifactsDir/$tarFileName" .
		echo "Version=$JDK_VERSION" >> "$hostJavaArtifactsDir/java-$JDK_VERSION-metadata.txt"
        return
    fi

    if shouldBuildSdk java $tarFileName || shouldOverwriteSdk || shouldOverwriteJavaSdk; then
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

		echo "Version=$JDK_VERSION" >> "$hostJavaArtifactsDir/java-$JDK_VERSION-metadata.txt"
		echo "JdkFullVersion=$JDK_VERSION+$JDK_BUILD_NUMBER" >> "$hostJavaArtifactsDir/java-$JDK_VERSION-metadata.txt"
    fi
}

shouldOverwriteNodeSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_JAVA" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Downloading Java SDK..."
echo
buildPlatform "$javaPlatformDir/versionsToBuild.txt" downloadJavaSdk

cp "$javaPlatformDir/defaultVersion.txt" $hostJavaArtifactsDir

ls -l $hostJavaArtifactsDir

