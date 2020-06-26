ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
ARG YARN_CACHE_BASE_TAG

FROM oryxdevmcr.azurecr.io/public/oryx/build:lts-versions AS main
ARG IMAGES_DIR
ARG BUILD_DIR
# End declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # Required for .NET Core 1.1
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

# A temporary folder to hold all content temporarily used to build this image.
# This folder is deleted in the final stage of building this image.
RUN mkdir -p ${IMAGES_DIR}
RUN mkdir -p ${BUILD_DIR}
ADD build ${BUILD_DIR}
ADD images ${IMAGES_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# Install .NET Core
FROM main AS dotnet-install
ARG BUILD_DIR
ARG IMAGES_DIR
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions

RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_11_SDK_VERSION \
    # To save disk space do not install packages for this old version which is soon going to be out of support
    INSTALL_PACKAGES=false \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

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
 # 1.1 sdk <-- 1.0 runtime's sdk
 && mkdir $NET_CORE_APP_10 \
 && ln -s $NET_CORE_APP_10 1.0 \
 && echo $DOT_NET_CORE_11_SDK_VERSION > $NET_CORE_APP_10/sdkVersion.txt \
 # 1.1 sdk <-- 1.1 runtime's sdk
 && mkdir $NET_CORE_APP_11 \
 && ln -s $NET_CORE_APP_11 1.1 \
 && ln -s 1.1 1 \
 && echo $DOT_NET_CORE_11_SDK_VERSION > $NET_CORE_APP_11/sdkVersion.txt \
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
FROM main AS node-install
ARG BUILD_DIR
ARG IMAGES_DIR
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN cd ${IMAGES_DIR} \ 
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ./installPlatform.sh nodejs 4.4.7 \
 && ./installPlatform.sh nodejs 4.5.0 \
 && ./installPlatform.sh nodejs 4.8.0 \
 && ./installPlatform.sh nodejs 6.2.2 \
 && ./installPlatform.sh nodejs 6.6.0 \
 && ./installPlatform.sh nodejs 6.9.3 \
 && ./installPlatform.sh nodejs 6.10.3 \
 && ./installPlatform.sh nodejs 6.11.0 \
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
 && ./installPlatform.sh nodejs 10.16.3 \
 && ./installPlatform.sh nodejs 12.16.3 \
 && ./installPlatform.sh nodejs 14.0.0 \
 && ./installPlatform.sh nodejs $NODE6_VERSION
RUN ${IMAGES_DIR}/build/installNpm.sh

RUN set -ex \
 && ln -s 4.4.7 /opt/nodejs/4.4 \
 && ln -s 4.5.0 /opt/nodejs/4.5 \
 && ln -s 4.8.0 /opt/nodejs/4.8 \
 && ln -s 4.8 /opt/nodejs/4 \
 && ln -s 6.2.2 /opt/nodejs/6.2 \
 && ln -s 6.6.0 /opt/nodejs/6.6 \
 && ln -s 6.9.3 /opt/nodejs/6.9 \
 && ln -s 6.10.3 /opt/nodejs/6.10 \
 && ln -s 6.11.0 /opt/nodejs/6.11 \ 
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
 && ln -s 10.16.3 /opt/nodejs/10.16 \
 && ln -s 12.16.3 /opt/nodejs/12.16 \
 && ln -s 14.0.0 /opt/nodejs/14.0 \
 && ln -s 14.0 /opt/nodejs/14 \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $NODE6_VERSION /opt/nodejs/6
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

FROM main AS python
ARG BUILD_DIR
ARG IMAGES_DIR
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
RUN cd ${IMAGES_DIR} \
    && . ${BUILD_DIR}/__pythonVersions.sh \
    && ./installPlatform.sh python $PYTHON27_VERSION \
    && ./installPlatform.sh python $PYTHON36_VERSION \
    && ./installPlatform.sh python $PYTHON37_VERSION \
    && ./installPlatform.sh python $PYTHON38_VERSION
RUN . ${BUILD_DIR}/__pythonVersions.sh && set -ex \
 && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN . ${BUILD_DIR}/__pythonVersions.sh && set -ex \
 && ln -sfn $PYTHON27_VERSION /opt/python/2.7 \
 && ln -sfn 2.7 /opt/python/2 \
 && ln -sfn $PYTHON36_VERSION /opt/python/3.6 \
 && ln -sfn $PYTHON37_VERSION /opt/python/3.7 \
 && ln -sfn $PYTHON38_VERSION /opt/python/3.8
  
# This stage is a no-op and exists to satisfy a pattern of building different
# stages in buildBuildImages.sh file
FROM dotnet-install AS buildscriptbuilder

FROM mcr.microsoft.com/oryx/base:build-yarn-cache-${YARN_CACHE_BASE_TAG} AS yarn-cache-base

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

FROM python AS final
ARG BUILD_DIR
ARG IMAGES_DIR
WORKDIR /

# Copy .NET Core related content
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
RUN ${IMAGES_DIR}/build/createFlattenedDotNetCoreInstall.sh
RUN cp ${IMAGES_DIR}/build/dotNetCoreAll.Readme.md /opt/dotnet/Readme.md

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Oryx depends on the run script generators for most of its
# `IProgrammingPlatform.GenerateBashRunScript()` implementations
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/
COPY --from=yarn-cache-base /usr/local/share/yarn-cache /usr/local/share/yarn-cache

# Copy PHP versions
RUN cd ${IMAGES_DIR} \
    && . ${BUILD_DIR}/__phpVersions.sh \
    && ./installPlatform.sh php $PHP56_VERSION \
    && ./installPlatform.sh php $PHP70_VERSION \
    && ./installPlatform.sh php $PHP72_VERSION \
    && ./installPlatform.sh php $PHP74_VERSION
RUN ln -s /opt/php/5.6 /opt/php/5

RUN rm -rf /tmp/oryx

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}
