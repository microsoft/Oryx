# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
ARG PYTHON_BASE_TAG
ARG PHP_BUILD_BASE_TAG
ARG YARN_CACHE_BASE_TAG
FROM oryxdevmcr.azurecr.io/public/oryx/build:slim AS main
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

RUN mkdir -p /tmp/scripts
COPY build/__dotNetCoreSdkVersions.sh /tmp/scripts
COPY build/__dotNetCoreRunTimeVersions.sh /tmp/scripts
COPY images/build/createFlattenedDotNetCoreInstall.sh /tmp/scripts
RUN chmod +x /tmp/scripts/createFlattenedDotNetCoreInstall.sh

# Install .NET Core
FROM main AS dotnet-install
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

COPY images/build/installDotNetCore.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installDotNetCore.sh

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_11_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_11_SDK_SHA512 \
    DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-dev-debian.9-x64.$DOTNET_SDK_VER.tar.gz \
    # To save disk space do not install packages for this old version which is soon going to be out of support
    INSTALL_PACKAGES=false \
    /tmp/scripts/installDotNetCore.sh

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_22_SDK_SHA512 \
    /tmp/scripts/installDotNetCore.sh

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_30_SDK_SHA512 \
    /tmp/scripts/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && dotnetDir=/opt/dotnet \
 && sdksDir=$dotnetDir/sdks \
 && runtimesDir=$dotnetDir/runtimes \
 && cd $runtimesDir \
 && . /tmp/scripts/__dotNetCoreSdkVersions.sh \
 && . /tmp/scripts/__dotNetCoreRunTimeVersions.sh \
 # 1.1 sdk <-- 1.0 runtime's sdk
 && mkdir $NET_CORE_APP_10 \
 && ln -s $sdksDir/$DOT_NET_CORE_11_SDK_VERSION $NET_CORE_APP_10/sdk \
 # 1.1 sdk <-- 1.1 runtime's sdk
 && mkdir $NET_CORE_APP_11 \
 && ln -s $sdksDir/$DOT_NET_CORE_11_SDK_VERSION $NET_CORE_APP_11/sdk \
 # 2.1 sdk <-- 2.0 runtime's sdk
 && mkdir $NET_CORE_APP_20 \
 && ln -s $sdksDir/$DOT_NET_CORE_21_SDK_VERSION $NET_CORE_APP_20/sdk \
 # 2.2 sdk <-- 2.2 runtime's sdk
 && mkdir $NET_CORE_APP_22 \
 && ln -s $sdksDir/$DOT_NET_CORE_22_SDK_VERSION $NET_CORE_APP_22/sdk \
 # 3.0 sdk <-- 3.0 runtime's sdk
 && mkdir $NET_CORE_APP_30 \
 && ln -s $sdksDir/$DOT_NET_CORE_30_SDK_VERSION $NET_CORE_APP_30/sdk

# Install Node.js, NPM, Yarn
FROM buildpack-deps:stretch AS node-install
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /tmp/scripts
COPY build/__nodeVersions.sh /tmp/scripts
RUN chmod a+x /tmp/scripts/__nodeVersions.sh \
 && . /tmp/scripts/__nodeVersions.sh \
 && curl -sL https://git.io/n-install | bash -s -- -ny - \
 && ~/n/bin/n -d 4.4.7 \
 && ~/n/bin/n -d 4.5.0 \
 && ~/n/bin/n -d 4.8.0 \
 && ~/n/bin/n -d 6.2.2 \
 && ~/n/bin/n -d 6.6.0 \
 && ~/n/bin/n -d 6.9.3 \
 && ~/n/bin/n -d 6.10.3 \
 && ~/n/bin/n -d 6.11.0 \
 && ~/n/bin/n -d 8.0.0 \
 && ~/n/bin/n -d 8.1.4 \
 && ~/n/bin/n -d 8.2.1 \
 && ~/n/bin/n -d 8.8.1 \
 && ~/n/bin/n -d 8.9.4 \
 && ~/n/bin/n -d 8.11.2 \
 && ~/n/bin/n -d 8.12.0 \
 && ~/n/bin/n -d 8.15.1 \
 && ~/n/bin/n -d 9.4.0 \
 && ~/n/bin/n -d 10.1.0 \
 && ~/n/bin/n -d 10.10.0 \
 && ~/n/bin/n -d 10.14.2 \
 && ~/n/bin/n -d 10.16.3 \
 && ~/n/bin/n -d 12.9.1 \
 && ~/n/bin/n -d $NODE6_VERSION \
 && mv /usr/local/n/versions/node /opt/nodejs \
 && rm -rf /usr/local/n ~/n
COPY images/build/installNpm.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installNpm.sh
RUN /tmp/scripts/installNpm.sh

###
# Python intermediate stages
# Docker doesn't support variables in `COPY --from`, so we're using intermediate stages
###
FROM mcr.microsoft.com/oryx/base:python-build-2.7-${PYTHON_BASE_TAG} AS py27-build-base
FROM mcr.microsoft.com/oryx/base:python-build-3.6-${PYTHON_BASE_TAG} AS py36-build-base
###
# End Python intermediate stages
###

FROM main AS python
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__pythonVersions.sh /tmp/scripts
COPY --from=py27-build-base /opt /opt
COPY --from=py36-build-base /opt /opt
RUN . /tmp/scripts/__pythonVersions.sh && set -ex \
 && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
  
# This stage is a no-op and exists to satisfy a pattern of building different
# stages in buildBuildImages.sh file
FROM dotnet-install AS buildscriptbuilder

###
# PHP intermediate stages
# Docker doesn't support variables in `COPY --from`, so we're using intermediate stages
###
FROM mcr.microsoft.com/oryx/base:php-build-5.6-${PHP_BUILD_BASE_TAG} AS php56-build-base
FROM mcr.microsoft.com/oryx/base:php-build-7.0-${PHP_BUILD_BASE_TAG} AS php70-build-base
FROM mcr.microsoft.com/oryx/base:php-build-7.2-${PHP_BUILD_BASE_TAG} AS php72-build-base
FROM mcr.microsoft.com/oryx/base:php-build-7.3-${PHP_BUILD_BASE_TAG} AS php73-build-base
###
# End PHP intermediate stages
###

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
WORKDIR /

# Copy .NET Core related content
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
RUN /tmp/scripts/createFlattenedDotNetCoreInstall.sh
COPY images/build/dotNetCoreAll.Readme.md /opt/dotnet/Readme.md

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Install PHP pre-reqs
COPY images/build/php/prereqs/installPrereqs.sh /tmp/scripts/installPhpPrereqs.sh
RUN chmod +x /tmp/scripts/installPhpPrereqs.sh
RUN /tmp/scripts/installPhpPrereqs.sh

# Oryx depends on the run script generators for most of its
# `IProgrammingPlatform.GenerateBashRunScript()` implementations
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/
COPY --from=yarn-cache-base /usr/local/share/yarn-cache /usr/local/share/yarn-cache

# Copy PHP versions
COPY --from=php56-build-base /opt /opt
COPY --from=php70-build-base /opt /opt
COPY --from=php72-build-base /opt /opt
COPY --from=php73-build-base /opt /opt

COPY build/__phpVersions.sh /tmp/scripts
RUN . /tmp/scripts/__phpVersions.sh \
 && ln -s $PHP73_VERSION /opt/php/lts

ENV PATH="$PATH:/opt/php/lts/bin"

RUN rm -rf /tmp/scripts

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}
