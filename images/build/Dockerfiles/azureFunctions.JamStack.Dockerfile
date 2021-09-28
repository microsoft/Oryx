ARG PARENT_DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/build:github-${PARENT_DEBIAN_FLAVOR} AS main
ARG DEBIAN_FLAVOR

COPY --from=support-files-image-for-build /tmp/oryx/ /tmp

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PATH="/home/jamstack/.dotnet/:/usr/local/go/bin:/opt/dotnet/lts:$PATH" \
    dotnet="/home/jamstack/.dotnet/dotnet"

RUN oryx prep --skip-detection --platforms-and-versions nodejs=12 \
    # https://github.com/microsoft/Oryx/issues/1032
    # Install .NET Core 3 SDKS
    && nugetPacakgesDir="/var/nuget" \
    && mkdir -p $nugetPacakgesDir \
    && NUGET_PACKAGES="$nugetPacakgesDir" \
    && . /tmp/build/__dotNetCoreSdkVersions.sh \
    && echo "$DEBIAN_FLAVOR" \
    && DOTNET_SDK_VER=$DOT_NET_CORE_31_SDK_VERSION /tmp/images/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPacakgesDir -type d -exec chmod 777 {} \; \
    && cd /opt/dotnet \
    && ln -s $DOT_NET_CORE_31_SDK_VERSION 3-lts \
    && ln -s 3-lts lts \
    && echo "jamstack" > /opt/oryx/.imagetype \
    && . /tmp/build/__goVersions.sh \
    && downloadedFileName="go${GO_VERSION}.linux-amd64.tar.gz" \
    && curl -SLsO https://golang.org/dl/$downloadedFileName \
    && mkdir -p /usr/local \
    && tar -xzf $downloadedFileName -C /usr/local \
    && rm -rf $downloadedFileName \ 
    && echo "jamstack" > /opt/oryx/.imagetype
    
RUN ./opt/tmp/build/createSymlinksForDotnet.sh