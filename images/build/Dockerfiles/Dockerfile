FROM oryxdevmcr.azurecr.io/public/oryx/build:lts-versions AS main

###
# Build run script generators (to be used by the `oryx run-script` command)
###
FROM golang:1.11-stretch as startupScriptGens

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}

RUN ./build.sh dotnetcore /opt/startupcmdgen/dotnet
RUN ./build.sh node       /opt/startupcmdgen/nodejs
RUN ./build.sh php        /opt/startupcmdgen/php
RUN ./build.sh python     /opt/startupcmdgen/python
###
# End build run script generators
###

# Install .NET Core
FROM main AS intermediate
COPY --from=support-files-image-for-build /tmp/oryx/ /opt/tmp
COPY --from=yarn-cache-base /usr/local/share/yarn-cache /opt/yarn-cache
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/
ARG BUILD_DIR="/opt/tmp/build"
ARG IMAGES_DIR="/opt/tmp/images"
RUN LANG="C.UTF-8" \
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir /opt/nuget
# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions
ENV NUGET_PACKAGES=/opt/nuget
RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /opt/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && sdksDir=/opt/dotnet/sdks \
 && cd $sdksDir \
 && ln -s 1.1 1 \
 && ln -s 3.0 3

RUN set -ex \
 && dotnetDir=/opt/dotnet \
 && sdksDir=$dotnetDir/sdks \
 && runtimesDir=$dotnetDir/runtimes \
 && cd $runtimesDir \
 && . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh \
 && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
 # 2.1 sdk <-- 2.0 runtime's sdk
 && mkdir $NET_CORE_APP_20 \
 && ln -s $NET_CORE_APP_20 2.0 \
 && echo $DOT_NET_CORE_21_SDK_VERSION > $NET_CORE_APP_20/sdkVersion.txt \
 # 2.2 sdk <-- 2.2 runtime's sdk
 && mkdir $NET_CORE_APP_22 \
 && ln -s $NET_CORE_APP_22 2.2 \
 && echo $DOT_NET_CORE_22_SDK_VERSION > $NET_CORE_APP_22/sdkVersion.txt \
 # 3.0 sdk <-- 3.0 runtime's sdk
 && mkdir $NET_CORE_APP_30 \
 && ln -s $NET_CORE_APP_30 3.0 \
 && echo $DOT_NET_CORE_30_SDK_VERSION > $NET_CORE_APP_30/sdkVersion.txt

# Install Node.js, NPM, Yarn
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN cd ${IMAGES_DIR} \ 
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ./installPlatform.sh nodejs 8.0.0 \
 && ./installPlatform.sh nodejs 8.1.4 \
 && ./installPlatform.sh nodejs 8.2.1 \
 && ./installPlatform.sh nodejs 8.8.1 \
 && ./installPlatform.sh nodejs 8.9.4 \
 && ./installPlatform.sh nodejs 8.11.2 \
 && ./installPlatform.sh nodejs 8.12.0 \
 && ./installPlatform.sh nodejs 8.15.1 \
 && ./installPlatform.sh nodejs 9.4.0 \
 && ./installPlatform.sh nodejs 10.1.0 \
 && ./installPlatform.sh nodejs 10.10.0 \
 && ./installPlatform.sh nodejs 10.14.2 \
 && ./installPlatform.sh nodejs 10.21.0 \
 && ./installPlatform.sh nodejs 12.18.2 \
RUN ${IMAGES_DIR}/build/installNpm.sh

RUN set -ex \
 && ln -s 8.0.0 /opt/nodejs/8.0 \
 && ln -s 8.1.4 /opt/nodejs/8.1 \
 && ln -s 8.2.1 /opt/nodejs/8.2 \
 && ln -s 8.8.1 /opt/nodejs/8.8 \
 && ln -s 8.9.4 /opt/nodejs/8.9 \
 && ln -s 8.11.2 /opt/nodejs/8.11 \
 && ln -s 8.12.0 /opt/nodejs/8.12 \
 && ln -s 8.15.1 /opt/nodejs/8.15 \
 && ln -s 9.4.0 /opt/nodejs/9.4 \
 && ln -s 9.4 /opt/nodejs/9 \
 && ln -s 10.1.0 /opt/nodejs/10.1 \
 && ln -s 10.10.0 /opt/nodejs/10.10 \
 && ln -s 10.14.2 /opt/nodejs/10.14 \
 && ln -s 10.21.0 /opt/nodejs/10.21 \
 && ln -s 12.18.2 /opt/nodejs/12.18 \
 && . ${BUILD_DIR}/__nodeVersions.sh \
RUN set -ex \
 && ln -s 2.15.9 /opt/npm/2.15 \
 && ln -s 2.15 /opt/npm/2 \
 && ln -s 3.9.5 /opt/npm/3.9 \
 && ln -s 3.10.10 /opt/npm/3.10 \
 && ln -s 3.10 /opt/npm/3 \
 && ln -s 5.0.3 /opt/npm/5.0 \
 && ln -s 5.3.0 /opt/npm/5.3 \
 && ln -s 5.4.2 /opt/npm/5.4 \
 && ln -s 5.6.0 /opt/npm/5.6 \
 && ln -s 5.6 /opt/npm/5

FROM main AS final
COPY --from=intermediate /opt /opt
# https://github.com/docker-library/python/issues/147
RUN PYTHONIOENCODING="UTF-8" \
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # Required for .NET Core 1.1
        libunwind8 \
    && rm -rf /var/lib/apt/lists/* \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    && mkdir -p /var/nuget \
    && cd /opt/nuget \
    && cp -rf . /var/nuget \
    && cd /opt \
    && rm -rf /opt/nuget \
    && mkdir -p /usr/local/share/yarn-cache \
    && cd /opt/yarn-cache \
    && cp -rf . /usr/local/share/yarn-cache \
    && cd /opt \
    && rm -rf /opt/yarn-cache \
    && chmod -R 777 /usr/local/share/yarn-cache \
    && cd $imagesDir \
    && . $buildDir/__pythonVersions.sh \
    && ./installPlatform.sh python $PYTHON27_VERSION \
    && ./installPlatform.sh python $PYTHON36_VERSION \
    && ./installPlatform.sh python $PYTHON37_VERSION \
    && ./installPlatform.sh python $PYTHON38_VERSION \
    && . $buildDir/__pythonVersions.sh && set -ex \
    && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && . $buildDir/__pythonVersions.sh && set -ex \
    && ln -sfn $PYTHON27_VERSION /opt/python/2.7 \
    && ln -sfn 2.7 /opt/python/2 \
    && ln -sfn $PYTHON36_VERSION /opt/python/3.6 \
    && ln -sfn $PYTHON37_VERSION /opt/python/3.7 \
    && ln -sfn $PYTHON38_VERSION /opt/python/3.8 \
    && $imagesDir/build/createFlattenedDotNetCoreInstall.sh \
    && cp $imagesDir/build/dotNetCoreAll.Readme.md /opt/dotnet/Readme.md \
    # Copy PHP versions
    && cd $imagesDir \
    && . $buildDir/__phpVersions.sh \
    && ./installPlatform.sh php $PHP56_VERSION \
    && ./installPlatform.sh php $PHP70_VERSION \
    && ./installPlatform.sh php $PHP72_VERSION \
    && ./installPlatform.sh php $PHP74_VERSION \
    && ln -s /opt/php/5.6 /opt/php/5 \
    && rm -rf $tmpDir
