# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
ARG PYTHON_BASE_TAG
FROM oryxdevmcr.azurecr.io/public/oryx/build-slim AS main
# End declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # Required for .NET Core 1.1
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /tmp/scripts
COPY build/installPlatform.sh /tmp/scripts

# Install .NET Core
FROM main AS dotnet-install
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

COPY build/__dotNetCoreSdkVersions.sh /tmp/scripts
COPY build/__dotNetCoreRunTimeVersions.sh /tmp/scripts
COPY images/build/installDotNetCore.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installDotNetCore.sh

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_11_SDK_VERSION \
    # To save disk space do not install packages for this old version which is soon going to be out of support
    INSTALL_PACKAGES=false \
    /tmp/scripts/installDotNetCore.sh

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION \
    /tmp/scripts/installDotNetCore.sh

RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION_PREVIEW_NAME \
    /tmp/scripts/installDotNetCore.sh

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
 && . /tmp/scripts/__dotNetCoreSdkVersions.sh \
 && . /tmp/scripts/__dotNetCoreRunTimeVersions.sh \
 # 1.1 sdk <-- 1.0 runtime's sdk
 && mkdir $NET_CORE_APP_10 \
 && ln -s $NET_CORE_APP_10 1.0 \
 && ln -s $sdksDir/$DOT_NET_CORE_11_SDK_VERSION $NET_CORE_APP_10/sdk \
 # 1.1 sdk <-- 1.1 runtime's sdk
 && mkdir $NET_CORE_APP_11 \
 && ln -s $NET_CORE_APP_11 1.1 \
 && ln -s 1.1 1 \
 && ln -s $sdksDir/$DOT_NET_CORE_11_SDK_VERSION $NET_CORE_APP_11/sdk \
 # 2.1 sdk <-- 2.0 runtime's sdk
 && mkdir $NET_CORE_APP_20 \
 && ln -s $NET_CORE_APP_20 2.0 \
 && ln -s $sdksDir/$DOT_NET_CORE_21_SDK_VERSION $NET_CORE_APP_20/sdk \
 # 2.2 sdk <-- 2.2 runtime's sdk
 && mkdir $NET_CORE_APP_22 \
 && ln -s $NET_CORE_APP_22 2.2 \
 && ln -s $sdksDir/$DOT_NET_CORE_22_SDK_VERSION $NET_CORE_APP_22/sdk \
 # 3.0 sdk <-- 3.0 runtime's sdk
 && mkdir $NET_CORE_APP_30 \
 && ln -s $NET_CORE_APP_30 3.0 \
 && ln -s 3.0 3 \
 && ln -s $sdksDir/$DOT_NET_CORE_30_SDK_VERSION $NET_CORE_APP_30/sdk

# Install Node.js, NPM, Yarn
FROM buildpack-deps:stretch AS node-install
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /tmp/scripts
COPY build/__nodeVersions.sh /tmp/scripts
COPY build/installPlatform.sh /tmp/scripts
RUN cd /tmp/scripts \
    && . /__nodeVersions.sh \
    && ./installPlatform.sh node 4.4.7 \
    && ./installPlatform.sh node 4.5.0 \
    && ./installPlatform.sh node 4.8.0 \
    && ./installPlatform.sh node 6.2.2 \
    && ./installPlatform.sh node 6.6.0 \
    && ./installPlatform.sh node 6.9.3 \
    && ./installPlatform.sh node 6.10.3 \
    && ./installPlatform.sh node 6.11.0 \
    && ./installPlatform.sh node 8.0.0 \
    && ./installPlatform.sh node 8.1.4 \
    && ./installPlatform.sh node 8.2.1 \
    && ./installPlatform.sh node 8.8.1 \
    && ./installPlatform.sh node 8.9.4 \
    && ./installPlatform.sh node 8.11.2 \
    && ./installPlatform.sh node 8.12.0 \
    && ./installPlatform.sh node 8.15.1 \
    && ./installPlatform.sh node 9.4.0 \
    && ./installPlatform.sh node 10.1.0 \
    && ./installPlatform.sh node 10.10.0 \
    && ./installPlatform.sh node 10.14.2 \
    && ./installPlatform.sh node $NODE6_VERSION \
    && ./installPlatform.sh node $NODE12_VERSION
COPY images/build/createNpmLinks.sh /tmp/scripts
RUN chmod +x /tmp/scripts/createNpmLinks.sh
RUN /tmp/scripts/createNpmLinks.sh

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
 && . /tmp/scripts/__nodeVersions.sh \
 && ln -s $NODE6_VERSION /opt/nodejs/$NODE6_MAJOR_MINOR_VERSION \ 
 && ln -s $NODE6_MAJOR_MINOR_VERSION /opt/nodejs/6 \
 && ln -s $NODE12_VERSION /opt/nodejs/$NODE12_MAJOR_MINOR_VERSION \
 && ln -s $NODE12_MAJOR_MINOR_VERSION /opt/nodejs/12
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

FROM buildpack-deps:stretch as pythonBits
COPY build/__pythonVersions.sh /tmp/scripts

FROM main AS python
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__pythonVersions.sh /tmp/scripts
COPY build/installPlatform.sh /tmp/scripts
RUN . /tmp/scripts/__pythonVersions.sh \
    && /tmp/scripts/installPlatform.sh python $PYTHON27_VERSION \
    && /tmp/scripts/installPlatform.sh python $PYTHON36_VERSION \
    && /tmp/scripts/installPlatform.sh python $PYTHON38_VERSION
RUN . /tmp/scripts/__pythonVersions.sh && set -ex \
 && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
# The link from PYTHON38_VERSION to 3.8.0 exists because "3.8.0b1" isn't a valid SemVer string.
RUN . /tmp/scripts/__pythonVersions.sh && set -ex \
 && ln -s $PYTHON27_VERSION /opt/python/2.7 \
 && ln -s 2.7 /opt/python/2 \
 && ln -s $PYTHON36_VERSION /opt/python/3.6 \
 && ln -s $PYTHON38_VERSION /opt/python/3.8.0 \
 && ln -s $PYTHON38_VERSION /opt/python/3.8
RUN set -ex \
 && cd /usr/local/bin \
 && cp -sn /opt/python/2/bin/* .
  
# This stage is a no-op and exists to satisfy a pattern of building different
# stages in buildBuildImages.sh file
FROM dotnet-install AS buildscriptbuilder

FROM buildpack-deps:stretch as phpBits
COPY build/__phpVersions.sh /tmp/scripts
COPY build/installPlatform.sh /tmp/scripts
RUN . /tmp/scripts/__phpVersions.sh \
    && /tmp/scripts/installPlatform.sh php $PHP56_VERSION \
    && /tmp/scripts/installPlatform.sh php $PHP70_VERSION \
    && /tmp/scripts/installPlatform.sh php $PHP72_VERSION \
    && /tmp/scripts/installPlatform.sh php $PHP73_VERSION

###
# Build run script generators (to be used by the `oryx run-script` command)
###
FROM golang:1.11-stretch as startupScriptGens

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}

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
COPY --from=dotnet-install /usr/local/bin /usr/local/bin

# Copy PHP versions
COPY build/__phpVersions.sh /tmp/scripts
COPY build/installPlatform.sh /tmp/scripts
RUN . /tmp/scripts/__phpVersions.sh \
    && /tmp/scripts/installPlatform.sh php $PHP56_VERSION \
    && /tmp/scripts/installPlatform.sh php $PHP70_VERSION \
    && /tmp/scripts/installPlatform.sh php $PHP72_VERSION

# Make sure the alias 'python' always refers to Python 2 by default
RUN cd /usr/local/bin \
 && rm -f python \
 && ln -sf /opt/python/2/bin/python python

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Copy PHP versions
COPY images/build/php/prereqs/installPrereqs.sh /tmp/scripts/installPhpPrereqs.sh
RUN chmod +x /tmp/scripts/installPhpPrereqs.sh
RUN /tmp/scripts/installPhpPrereqs.sh

# Oryx depends on the run script generators for most of its
# `IProgrammingPlatform.GenerateBashRunScript()` implementations
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/
COPY --from=mcr.microsoft.com/oryx/build-yarn-cache:20190802.1 /usr/local/share/yarn-cache /usr/local/share/yarn-cache

RUN ln -s /opt/php/5.6 /opt/php/5 \
 && ln -s /opt/php/7.3 /opt/php/7 \
 && ln -s /opt/php/7 /opt/php/lts \
 && ln -s /opt/php/lts/bin/php /usr/local/bin/php

RUN rm -rf /tmp/scripts

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
