# Start declaration of Build-Arg to determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD
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
COPY images/build/installPlatform.sh /tmp/scripts
RUN chmod +x /tmp/scripts/installPlatform.sh

# This is the folder containing 'links' to versions of sdks present under '/opt' folder
# These versions are typically the LTS or stable versions of those platforms.
RUN mkdir -p /opt/oryx/defaultversions

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
    /tmp/scripts/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && sdksDir=/opt/dotnet/sdks \
 && cd $sdksDir \
 && ln -s 2.1 2

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
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
COPY build/__nodeVersions.sh /tmp/scripts
COPY images/build/createNpmLinks.sh /tmp/scripts
RUN cd /tmp/scripts \
 && . ./__nodeVersions.sh \
 && ./installPlatform.sh nodejs $NODE8_VERSION \
 && ./installPlatform.sh nodejs $NODE10_VERSION \
 && chmod +x ./createNpmLinks.sh \
 && ./createNpmLinks.sh

COPY images/receivePgpKeys.sh /tmp/scripts
RUN cd /tmp/scripts \
 && chmod +x ./receivePgpKeys.sh \
 && . ./__nodeVersions.sh \
 && ./receivePgpKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

RUN set -ex \
 && . /tmp/scripts/__nodeVersions.sh \
 && cd /opt/nodejs \
 && ln -s $NODE8_VERSION $NODE8_MAJOR_MINOR_VERSION \
 && ln -s $NODE8_MAJOR_MINOR_VERSION 8 \
 && ln -s $NODE10_VERSION $NODE10_MAJOR_MINOR_VERSION \
 && ln -s $NODE10_MAJOR_MINOR_VERSION 10 \
 && ln -s 10 lts
RUN set -ex \
 && cd /opt/npm \
 && ln -s 6.9.0 6.9 \
 && ln -s 6.9 6 \
 && ln -s 6 latest
RUN set -ex \
 && cd /opt/yarn \
 && . /tmp/scripts/__nodeVersions.sh \
 && ln -s $YARN_VERSION stable \
 && ln -s $YARN_VERSION latest \
 && ln -s $YARN_VERSION $YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION $YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

FROM main AS python
# It's not clear whether these are needed at runtime...
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__pythonVersions.sh /tmp/scripts
RUN set -ex \
 && cd /tmp/scripts \
 && . ./__pythonVersions.sh \
 && ./installPlatform.sh python $PYTHON37_VERSION \
 && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
# The link from PYTHON38_VERSION to 3.8.0 exists because "3.8.0b1" isn't a valid SemVer string.
RUN set -ex \
 && . /tmp/scripts/__pythonVersions.sh \
 && cd /opt/python \
 && ln -s $PYTHON37_VERSION latest \
 && ln -s $PYTHON37_VERSION 3.7 \
 && ln -s 3.7 3
RUN set -ex \
 && cd /opt/oryx/defaultversions \
 && cp -sn /opt/python/3/bin/* . \
 # Make sure the alias 'python' always refers to Python 3 by default
 && ln -sf /opt/python/3/bin/python python

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
RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

FROM python AS final
WORKDIR /

ENV PATH=$PATH:/opt/oryx/defaultversions
COPY images/build/benv.sh /opt/oryx/defaultversions/benv
RUN chmod +x /opt/oryx/defaultversions/benv
RUN mkdir -p /usr/local/share/pip-cache/lib
RUN chmod -R 777 /usr/local/share/pip-cache

# Copy .NET Core related content
ENV NUGET_XMLDOC_MODE=skip \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
COPY --from=dotnet-install /usr/local/bin /opt/oryx/defaultversions
# Grant read-write permissions to the nuget folder so that dotnet restore
# can write into it.
RUN chmod a+rw /var/nuget

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt
COPY --from=node-install /links/ /opt/oryx/defaultversions

# Build script generator content. Docker doesn't support variables in --from
# so we are building an extra stage to copy binaries from correct build stage
COPY --from=buildscriptbuilder /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/defaultversions/oryx

RUN rm -rf /tmp/scripts

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}

ENTRYPOINT [ "benv" ]
