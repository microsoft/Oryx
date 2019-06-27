# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
ARG PYTHON_BASE_TAG
ARG PHP_BUILD_BASE_TAG
FROM buildpack-deps:stretch AS main
# End declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
        jq \
        make \
        unzip \
        # The tools in this package are used when installing packages for Python
        build-essential \
        # Required for Microsoft SQL Server
        unixodbc-dev \
        # Required for PostgreSQL
        libpq-dev \
        # Required for mysqlclient
        default-libmysqlclient-dev \
        # Required for .NET Core 1.1
        libunwind8 \
        # Required for ts
        moreutils \
        rsync \
        zip \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Core
FROM main AS dotnet-install
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu57 \
        liblttng-ust0 \
        libssl1.0.2 \
        libstdc++6 \
        zlib1g \
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
    && rm -rf /var/lib/apt/lists/*

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
	NUGET_XMLDOC_MODE=skip \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget

RUN mkdir /var/nuget
COPY build/__dotNetCoreSdkVersions.sh /tmp
COPY build/__dotNetCoreRunTimeVersions.sh /tmp
COPY images/build/installDotNetCore.sh /
RUN chmod +x /installDotNetCore.sh

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions

RUN . /tmp/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_11_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_11_SDK_SHA512 \
    DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-dev-debian.9-x64.$DOTNET_SDK_VER.tar.gz \
    # To save disk space do not install packages for this old version which is soon going to be out of support
    INSTALL_PACKAGES=false \
    /installDotNetCore.sh

RUN . /tmp/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_21_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_21_SDK_SHA512 \
    /installDotNetCore.sh

RUN . /tmp/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_22_SDK_SHA512 \
    /installDotNetCore.sh

RUN . /tmp/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION_PREVIEW_NAME \
    DOTNET_SDK_SHA=$DOT_NET_CORE_30_SDK_SHA512 \
    /installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && sdksDir=/opt/dotnet/sdks \
 && cd $sdksDir \
 && ln -s 1.1 1 \
 && ln -s 2.1 2 \
 && ln -s 3.0 3

RUN set -ex \
 && dotnetDir=/opt/dotnet \
 && sdksDir=$dotnetDir/sdks \
 && runtimesDir=$dotnetDir/runtimes \
 && mkdir -p $runtimesDir \
 && cd $runtimesDir \
 && . /tmp/__dotNetCoreSdkVersions.sh \
 && . /tmp/__dotNetCoreRunTimeVersions.sh \
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
 # 2.1 sdk <-- 2.1 runtime's sdk
 && mkdir $NET_CORE_APP_21 \
 && ln -s $NET_CORE_APP_21 2.1 \
 && ln -s 2.1 2 \
 && ln -s $sdksDir/$DOT_NET_CORE_21_SDK_VERSION $NET_CORE_APP_21/sdk \
 # 2.2 sdk <-- 2.2 runtime's sdk
 && mkdir $NET_CORE_APP_22 \
 && ln -s $NET_CORE_APP_22 2.2 \
 && ln -s $sdksDir/$DOT_NET_CORE_22_SDK_VERSION $NET_CORE_APP_22/sdk \
 # 3.0 sdk <-- 3.0 runtime's sdk
 && mkdir $NET_CORE_APP_30 \
 && ln -s $NET_CORE_APP_30 3.0 \
 && ln -s 3.0 3 \
 && ln -s $sdksDir/$DOT_NET_CORE_30_SDK_VERSION $NET_CORE_APP_30/sdk \
 # LTS sdk <-- LTS runtime's sdk
 && ln -s 2.1 lts \
 && ltsSdk=$(readlink lts/sdk) \
 && ln -s $ltsSdk/dotnet /usr/local/bin/dotnet

# Install Node.js, NPM, Yarn
FROM main AS node-install
COPY build/__nodeVersions.sh /tmp
RUN chmod a+x /tmp/__nodeVersions.sh \
 && . /tmp/__nodeVersions.sh \
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
 && ~/n/bin/n -d $NODE6_VERSION \
 && ~/n/bin/n -d $NODE8_VERSION \
 && ~/n/bin/n -d $NODE10_VERSION \
 && mv /usr/local/n/versions/node /opt/nodejs \
 && rm -rf /usr/local/n ~/n
RUN set -e \
 && for ver in `ls /opt/nodejs`; do \
        nodeModulesDir="/opt/nodejs/$ver/lib/node_modules"; \
        npm_ver=`jq -r .version $nodeModulesDir/npm/package.json`; \
        if [ ! "$npm_ver" = "${npm_ver#6.}" ]; then \
            echo "Upgrading node $ver's npm version from $npm_ver to 6.9.0"; \
            cd $nodeModulesDir; \
            PATH="/opt/nodejs/$ver/bin:$PATH" \
            "$nodeModulesDir/npm/bin/npm-cli.js" install npm@6.9.0; \
            echo; \
        fi; \
    done
RUN set -ex \
 && for ver in `ls /opt/nodejs`; do \
        npm_ver=`jq -r .version /opt/nodejs/$ver/lib/node_modules/npm/package.json`; \
        if [ ! -d /opt/npm/$npm_ver ]; then \
            mkdir -p /opt/npm/$npm_ver; \
            ln -s /opt/nodejs/$ver/lib/node_modules /opt/npm/$npm_ver/node_modules; \
            ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npm /opt/npm/$npm_ver/npm; \
            if [ -e /opt/nodejs/$ver/lib/node_modules/npm/bin/npx ]; then \
                chmod +x /opt/nodejs/$ver/lib/node_modules/npm/bin/npx; \
                ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npx /opt/npm/$npm_ver/npx; \
            fi; \
        fi; \
    done

RUN set -ex \
 && . /tmp/__nodeVersions.sh \
 && GPG_KEY=6A010C5166006599AA17F08146C2130DFD2497F5 \
 && for i in {1..5}; do \
      gpg --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys "$GPG_KEY" || \
      gpg --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys "$GPG_KEY" || \
      gpg --keyserver hkp://pgp.mit.edu:80 --recv-keys "$GPG_KEY"; \
      if [ $? -eq 0 ]; then break; fi \
    done \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

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
 && . /tmp/__nodeVersions.sh \
 && ln -s $NODE6_VERSION /opt/nodejs/$NODE6_MAJOR_MINOR_VERSION \ 
 && ln -s $NODE6_MAJOR_MINOR_VERSION /opt/nodejs/6 \
 && ln -s $NODE8_VERSION /opt/nodejs/$NODE8_MAJOR_MINOR_VERSION \
 && ln -s $NODE8_MAJOR_MINOR_VERSION /opt/nodejs/8 \
 && ln -s $NODE10_VERSION /opt/nodejs/$NODE10_MAJOR_MINOR_VERSION \
 && ln -s $NODE10_MAJOR_MINOR_VERSION /opt/nodejs/10 \
 && ln -s 10 /opt/nodejs/lts
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
 && ln -s 5.6 /opt/npm/5 \
 && ln -s 6.9.0 /opt/npm/6.9 \
 && ln -s 6.9 /opt/npm/6 \
 && ln -s 6 /opt/npm/latest
RUN set -ex \
 && . /tmp/__nodeVersions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

###
# Python intermediate stages
# Docker doesn't support variables in `COPY --from`, so we're using intermediate stages
###
FROM mcr.microsoft.com/oryx/python-build-base:2.7-${PYTHON_BASE_TAG} AS py27-build-base
FROM mcr.microsoft.com/oryx/python-build-base:3.6-${PYTHON_BASE_TAG} AS py36-build-base
FROM mcr.microsoft.com/oryx/python-build-base:3.7-${PYTHON_BASE_TAG} AS py37-build-base
FROM mcr.microsoft.com/oryx/python-build-base:3.8-${PYTHON_BASE_TAG} AS py38-build-base
###
# End Python intermediate stages
###

FROM main AS python
# It's not clear whether these are needed at runtime...
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__pythonVersions.sh /tmp
COPY --from=py27-build-base /opt /opt
COPY --from=py36-build-base /opt /opt
COPY --from=py37-build-base /opt /opt
COPY --from=py38-build-base /opt /opt
RUN . /tmp/__pythonVersions.sh && set -ex \
 && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
# The link from PYTHON38_VERSION to 3.8.0 exists because "3.8.0b1" isn't a valid SemVer string.
RUN . /tmp/__pythonVersions.sh && set -ex \
 && ln -s $PYTHON27_VERSION /opt/python/2.7 \
 && ln -s 2.7 /opt/python/2 \
 && ln -s $PYTHON36_VERSION /opt/python/3.6 \
 && ln -s $PYTHON37_VERSION /opt/python/latest \
 && ln -s $PYTHON37_VERSION /opt/python/3.7 \
 && ln -s $PYTHON38_VERSION /opt/python/3.8.0 \
 && ln -s $PYTHON38_VERSION /opt/python/3.8 \
 && ln -s 3.7 /opt/python/3
RUN set -ex \
 && cd /usr/local/bin \
 && cp -sn /opt/python/2/bin/* . \
 && cp -sn /opt/python/3/bin/* . \
 # Make sure the alias 'python' always refers to Python 2 by default
 && ln -sf /opt/python/2/bin/python python

# This stage is used only when building locally
FROM dotnet-install AS buildscriptbuilder
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Common /usr/oryx/src/Common
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
# This statement copies signed oryx binaries from during agent build.
# For local/dev contents of blank/empty directory named binaries are getting copied
COPY binaries /opt/buildscriptgen/
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG AGENTBUILD=${AGENTBUILD}
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
COPY images/build/benv.sh /usr/local/bin/benv
RUN chmod +x /usr/local/bin/benv
RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

###
# PHP intermediate stages
# Docker doesn't support variables in `COPY --from`, so we're using intermediate stages
###
FROM mcr.microsoft.com/oryx/php-build-base:5.6-${PHP_BUILD_BASE_TAG} AS php56-build-base
FROM mcr.microsoft.com/oryx/php-build-base:7.0-${PHP_BUILD_BASE_TAG} AS php70-build-base
FROM mcr.microsoft.com/oryx/php-build-base:7.2-${PHP_BUILD_BASE_TAG} AS php72-build-base
FROM mcr.microsoft.com/oryx/php-build-base:7.3-${PHP_BUILD_BASE_TAG} AS php73-build-base
###
# End PHP intermediate stages
###

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

COPY images/build/benv.sh /usr/local/bin/benv
RUN chmod +x /usr/local/bin/benv

# Copy .NET Core related content
ENV NUGET_XMLDOC_MODE=skip \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
COPY --from=dotnet-install /usr/local/bin /usr/local/bin
# Grant read-write permissions to the nuget folder so that dotnet restore
# can write into it.
RUN chmod a+rw /var/nuget

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt
COPY --from=node-install /links/ /usr/local/bin
COPY --from=mcr.microsoft.com/oryx/build-yarn-cache:20190326.8 /usr/local/share/yarn-cache /usr/local/share/yarn-cache

# Copy PHP versions
COPY images/build/php/prereqs/installPrereqs.sh /tmp/php/installPrereqs.sh
RUN . /tmp/php/installPrereqs.sh

COPY --from=php56-build-base /opt /opt
COPY --from=php70-build-base /opt /opt
COPY --from=php72-build-base /opt /opt
COPY --from=php73-build-base /opt /opt

RUN ln -s /opt/php/5.6 /opt/php/5 \
 && ln -s /opt/php/7.3 /opt/php/7 \
 && ln -s /opt/php/7 /opt/php/lts \
 && ln -s /opt/php/lts/bin/php /usr/local/bin/php

# Build script generator content. Docker doesn't support variables in --from
# so we are building an extra stage to copy binaries from correct build stage
COPY --from=buildscriptbuilder /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /usr/local/bin/oryx

# Oryx depends on the run script generators for most of its
# `IProgrammingPlatform.GenerateBashRunScript()` implementations
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}

ENTRYPOINT [ "benv" ]
