ARG PARENT_DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/build:github-${PARENT_DEBIAN_FLAVOR} AS main
ARG DEBIAN_FLAVOR

COPY --from=support-files-image-for-build /tmp/oryx/ /tmp

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PATH="/home/jamstack/.dotnet/:/usr/local/go/bin:/opt/dotnet/lts:/opt/python/latest/bin:$PATH" \
    dotnet="/home/jamstack/.dotnet/dotnet" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8"

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
    && rm -rf $downloadedFileName

RUN set -ex \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    # Install Python SDKs
    # Upgrade system python
    && PYTHONIOENCODING="UTF-8" \
    # It's not clear whether these are needed at runtime...
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        build-essential \
        python3-pip \
        swig3.0 \
        tk-dev \
        uuid-dev \
    && rm -rf /var/lib/apt/lists/* \
    && pip3 install pip --upgrade \
    && pip install --upgrade cython \
    && pip3 install --upgrade cython \
    && . $buildDir/__pythonVersions.sh \
    && $imagesDir/installPlatform.sh python $PYTHON38_VERSION \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -s $PYTHON38_VERSION 3.8 \
    && ln -s $PYTHON38_VERSION latest \
    && ln -s $PYTHON38_VERSION stable \
    && echo "jamstack" > /opt/oryx/.imagetype
    
RUN ./opt/tmp/build/createSymlinksForDotnet.sh