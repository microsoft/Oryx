ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/build:github-${DEBIAN_FLAVOR} AS main
ARG DEBIAN_FLAVOR

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    PATH="/usr/local/go/bin:/opt/dotnet/lts:$PATH"

COPY --from=support-files-image-for-build /tmp/oryx/ /tmp
RUN oryx prep --skip-detection --platforms-and-versions nodejs=12 \
    # https://github.com/microsoft/Oryx/issues/1032
    # Install .NET Core 3 SDKS
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    && nugetPacakgesDir="/var/nuget" \
    && mkdir -p $nugetPacakgesDir \
    && NUGET_PACKAGES="$nugetPacakgesDir" \
    && . $buildDir/__dotNetCoreSdkVersions.sh \
    && DOTNET_SDK_VER=$DOT_NET_31_SDK_VERSION $imagesDir/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPacakgesDir -type d -exec chmod 777 {} \; \
    && cd /opt/dotnet \
    && ln -s $DOT_NET_31_SDK_VERSION 3-lts \
    && ln -s 3-lts lts \
    && echo "jamstack" > /opt/oryx/.imagetype \
    && . /tmp/build/__goVersions.sh \
    && downloadedFileName="go${GO_VERSION}.linux-amd64.tar.gz" \
    && curl -SLsO https://golang.org/dl/$downloadedFileName \
    && tar -C /usr/local -xzf $downloadedFileName \
    && rm -rf $downloadedFileName
