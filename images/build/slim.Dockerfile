# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
ARG PYTHON_BASE_TAG
ARG PHP_BUILD_BASE_TAG
FROM buildpack-deps:stretch AS main
# End declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Oryx's path is at the end of the PATH environment variable value and so earlier presence
# of python in the path folders (in this case /usr/bin) will cause Oryx's platform sdk to be not
# picked up.
RUN rm -rf /usr/bin/python*
RUN rm -rf /usr/bin/pydoc*

# Install basic build tools
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        git \
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
        # Required for ts
        moreutils \
        rsync \
        zip \
    && rm -rf /var/lib/apt/lists/*

# A temporary folder to hold all scripts temporarily used to build this image. 
# This folder is deleted in the final stage of building this image.
RUN mkdir -p /tmp/scripts

# This is the folder containing 'links' to benv and build script generator
RUN mkdir -p /opt/oryx

# Install .NET Core
FROM main AS dotnet-install
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu57 \
        liblttng-ust0 \
        libssl1.0.2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
	NUGET_XMLDOC_MODE=skip \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget

RUN mkdir /var/nuget
COPY build/__dotNetCoreSdkVersions.sh /tmp/scripts
COPY build/__dotNetCoreRunTimeVersions.sh /tmp/scripts
COPY images/build/installDotNetCore.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installDotNetCore.sh

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions
RUN . /tmp/scripts/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_21_SDK_VERSION \
    DOTNET_SDK_SHA=$DOT_NET_CORE_21_SDK_SHA512 \
    /tmp/scripts/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && sdksDir=/opt/dotnet/sdks \
 && cd $sdksDir \
 && ln -s 2.1 2 \
 && ln -s 2 lts

RUN set -ex \
 && dotnetDir=/opt/dotnet \
 && sdksDir=$dotnetDir/sdks \
 && runtimesDir=$dotnetDir/runtimes \
 && mkdir -p $runtimesDir \
 && cd $runtimesDir \
 && . /tmp/scripts/__dotNetCoreSdkVersions.sh \
 && . /tmp/scripts/__dotNetCoreRunTimeVersions.sh \
 && mkdir $NET_CORE_APP_21 \
 && ln -s $NET_CORE_APP_21 2.1 \
 && ln -s 2.1 2 \
 && ln -s $sdksDir/$DOT_NET_CORE_21_SDK_VERSION $NET_CORE_APP_21/sdk \
 # LTS sdk <-- LTS runtime's sdk
 && ln -s 2.1 lts \
 && ltsSdk=$(readlink lts/sdk) \
 && ln -s $ltsSdk/dotnet /usr/local/bin/dotnet

# Install Node.js, NPM, Yarn
FROM main AS node-install
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
COPY build/__nodeVersions.sh /tmp/scripts
RUN chmod a+x /tmp/scripts/__nodeVersions.sh \
 && . /tmp/scripts/__nodeVersions.sh \
 && curl -sL https://git.io/n-install | bash -s -- -ny - \
 && ~/n/bin/n -d $NODE8_VERSION \
 && ~/n/bin/n -d $NODE10_VERSION \
 && ~/n/bin/n -d $NODE12_VERSION \
 && mv /usr/local/n/versions/node /opt/nodejs \
 && rm -rf /usr/local/n ~/n
COPY images/build/installNpm.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installNpm.sh
RUN /tmp/scripts/installNpm.sh
COPY images/receivePgpKeys.sh /tmp/scripts
RUN chmod +x /tmp/scripts/receivePgpKeys.sh
RUN set -ex \
 && . /tmp/scripts/__nodeVersions.sh \
 && /tmp/scripts/receivePgpKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

RUN set -ex \
 && . /tmp/scripts/__nodeVersions.sh \
 && ln -s $NODE8_VERSION /opt/nodejs/$NODE8_MAJOR_MINOR_VERSION \
 && ln -s $NODE8_MAJOR_MINOR_VERSION /opt/nodejs/8 \
 && ln -s $NODE10_VERSION /opt/nodejs/$NODE10_MAJOR_MINOR_VERSION \
 && ln -s $NODE10_MAJOR_MINOR_VERSION /opt/nodejs/10 \
 && ln -s $NODE12_VERSION /opt/nodejs/$NODE12_MAJOR_MINOR_VERSION \
 && ln -s $NODE12_MAJOR_MINOR_VERSION /opt/nodejs/12 \
 && ln -s 12 /opt/nodejs/lts
RUN set -ex \
 && ln -s 6.9.0 /opt/npm/6.9 \
 && ln -s 6.9 /opt/npm/6 \
 && ln -s 6 /opt/npm/latest
RUN set -ex \
 && . /tmp/scripts/__nodeVersions.sh \
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
FROM mcr.microsoft.com/oryx/python-build-base:3.7-${PYTHON_BASE_TAG} AS py37-build-base
FROM mcr.microsoft.com/oryx/python-build-base:3.8-${PYTHON_BASE_TAG} AS py38-build-base
###
# End Python intermediate stages
###

FROM main AS python
# It's not clear whether these are needed at runtime...
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
    && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__pythonVersions.sh /tmp/scripts
COPY --from=py37-build-base /opt /opt
COPY --from=py38-build-base /opt /opt
RUN . /tmp/scripts/__pythonVersions.sh && set -ex \
 && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN . /tmp/scripts/__pythonVersions.sh && set -ex \
 && ln -s $PYTHON37_VERSION /opt/python/3.7 \
 && ln -s $PYTHON38_VERSION /opt/python/3.8 \
 && ln -s $PYTHON38_VERSION /opt/python/latest \
 && ln -s $PYTHON38_VERSION /opt/python/stable \
 && ln -s 3.8 /opt/python/3

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
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ARG AGENTBUILD=${AGENTBUILD}
RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

FROM python AS final
WORKDIR /

ENV PATH="$PATH:/opt/oryx:/opt/nodejs/lts/bin:/opt/dotnet/sdks/lts:/opt/python/latest/bin:/opt/yarn/stable/bin"
COPY images/build/benv.sh /opt/oryx/benv
RUN chmod +x /opt/oryx/benv
RUN mkdir -p /usr/local/share/pip-cache/lib
RUN chmod -R 777 /usr/local/share/pip-cache

# Copy .NET Core related content
ENV NUGET_XMLDOC_MODE=skip \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
# Grant read-write permissions to the nuget folder so that dotnet restore
# can write into it.
RUN chmod a+rw /var/nuget

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Build script generator content. Docker doesn't support variables in --from
# so we are building an extra stage to copy binaries from correct build stage
COPY --from=buildscriptbuilder /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx

RUN rm -rf /tmp/scripts

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}

ENTRYPOINT [ "benv" ]
