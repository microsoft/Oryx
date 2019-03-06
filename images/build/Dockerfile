# Start declaration of Build-Arg to determine where the image is getting built (devops agents or local)
ARG BUILDSCRIPT_SOURCE=buildscriptbuilder
ARG AGENTBUILD
FROM buildpack-deps:stable AS main
# End declaration of Build-Arg to determine where the image is getting built (devops agents or local)

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
        jq \
        make \
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
    ASPNETCORE_URLS=http://+:80 \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget

RUN mkdir /var/nuget
COPY images/build/dotnetcore-install.sh /
RUN chmod +x /dotnetcore-install.sh

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions

RUN DOTNET_SDK_VER=1.1.12 \
    DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-dev-debian.9-x64.$DOTNET_SDK_VER.tar.gz \
    # To save disk space do not install packages for this old version which is soon going to be out of support
    INSTALL_PACKAGES=false \
    /dotnetcore-install.sh

RUN DOTNET_SDK_VER=2.1.504 \
    DOTNET_SDK_SHA=93318A533DA67DF22CAE5C4BBB384ABD820B37D66E473DF08DA26231D0DF0D0420661E4E587CED0954EE77EB3AF532E52D0AB1DDD9528AA0EE74B4EA1C27702D \
    /dotnetcore-install.sh

RUN DOTNET_SDK_VER=2.2.104 \
    DOTNET_SDK_SHA=FD03CC4ABEA849EE5E05A035E2888C71D8842E64389DD94D7301E0FCFC189CBED99FE84A6174B657FFE3D328FAA761972C061A339246F63C9BA8FA31EAD2A1B0 \
    /dotnetcore-install.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /var/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && ln -s 1.1 /opt/dotnet/1 \
 && ln -s 2.1 /opt/dotnet/2 \
 && ln -s 2 /opt/dotnet/lts \
 && ln -s /opt/dotnet/lts/dotnet /usr/local/bin/dotnet

# Install Node.js, NPM, Yarn
FROM main AS node-install
RUN curl -sL https://git.io/n-install | bash -s -- -ny - \
 && ~/n/bin/n -d 4.4.7 \
 && ~/n/bin/n -d 4.5.0 \
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
 && ~/n/bin/n -d 10.14.1 \
 && ~/n/bin/n -d 10.15.2 \
 && mv /usr/local/n/versions/node /opt/nodejs \
 && rm -rf /usr/local/n ~/n
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

COPY build/__node-versions.sh /tmp
RUN set -ex \
 && . /tmp/__node-versions.sh \
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
 && ln -s 4.5 /opt/nodejs/4 \
 && ln -s 6.2.2 /opt/nodejs/6.2 \
 && ln -s 6.6.0 /opt/nodejs/6.6 \
 && ln -s 6.9.3 /opt/nodejs/6.9 \
 && ln -s 6.10.3 /opt/nodejs/6.10 \
 && ln -s 6.11.0 /opt/nodejs/6.11 \
 && ln -s 6.11 /opt/nodejs/6 \
 && ln -s 8.0.0 /opt/nodejs/8.0 \
 && ln -s 8.1.4 /opt/nodejs/8.1 \
 && ln -s 8.2.1 /opt/nodejs/8.2 \
 && ln -s 8.8.1 /opt/nodejs/8.8 \
 && ln -s 8.9.4 /opt/nodejs/8.9 \
 && ln -s 8.11.2 /opt/nodejs/8.11 \
 && ln -s 8.12.0 /opt/nodejs/8.12 \
 && ln -s 8.15.1 /opt/nodejs/8.15 \
 && ln -s 8.15 /opt/nodejs/8 \
 && ln -s 9.4.0 /opt/nodejs/9.4 \
 && ln -s 9.4 /opt/nodejs/9 \
 && ln -s 10.1.0 /opt/nodejs/10.1 \
 && ln -s 10.10.0 /opt/nodejs/10.10 \
 && ln -s 10.14.1 /opt/nodejs/10.14 \
 && ln -s 10.15.2 /opt/nodejs/10.15 \
 && ln -s 10.15 /opt/nodejs/10 \
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
 && ln -s 6.4.1 /opt/npm/latest \
 && ln -s 6.4.1 /opt/npm/6.4 \
 && ln -s 6.4 /opt/npm/6
RUN set -ex \
 && . /tmp/__node-versions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

# Install Python build prerequisites
FROM buildpack-deps:stable AS python-build-prereqs
COPY build/__python-versions.sh /tmp/
COPY images/build/python-build.sh /
RUN chmod +x /python-build.sh
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      tk-dev \
      uuid-dev

# Build Python 2.7
FROM python-build-prereqs AS python2.7-build
WORKDIR /usr/src/python
ENV GPG_KEY C01E1CAD5EA2C4F0B8E3571504C367C218ADD4FF
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON27_VERSION PIP_VERSION=$PIP_VERSION /python-build.sh

# Build Python 3.5
FROM python-build-prereqs AS python3.5-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.5.6
ENV GPG_KEY 97FC712E4C024BBEA48A61ED3A5CA953F73C700D
RUN . /tmp/__python-versions.sh && PIP_VERSION=$PIP_VERSION /python-build.sh

# Build Python 3.6
FROM python-build-prereqs AS python3.6-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.6.7
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
RUN . /tmp/__python-versions.sh && PIP_VERSION=$PIP_VERSION /python-build.sh

# Build Python 3.7
FROM python-build-prereqs AS python3.7-build
WORKDIR /usr/src/python
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON37_VERSION PIP_VERSION=$PIP_VERSION /python-build.sh

FROM main AS python
# It's not clear whether these are needed at runtime...
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY build/__python-versions.sh /tmp
COPY --from=python2.7-build /opt /opt
COPY --from=python3.5-build /opt /opt
COPY --from=python3.6-build /opt /opt
COPY --from=python3.7-build /opt /opt
RUN . /tmp/__python-versions.sh && set -ex \
 && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.5.6/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.6.7/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN . /tmp/__python-versions.sh && set -ex \
 && ln -s $PYTHON27_VERSION /opt/python/2.7 \
 && ln -s 2.7 /opt/python/2 \
 && ln -s 3.5.6 /opt/python/3.5 \
 && ln -s 3.6.7 /opt/python/3.6 \
 && ln -s $PYTHON37_VERSION /opt/python/latest \
 && ln -s $PYTHON37_VERSION /opt/python/3.7 \
 && ln -s 3.7 /opt/python/3
RUN set -ex \
 && cd /usr/local/bin \
 && cp -sn /opt/python/2/bin/* . \
 && cp -sn /opt/python/3/bin/* . \
 # Make sure the alias 'python' always refers to python2 by default.
 && ln -sf /opt/python/2/bin/python python

FROM debian:stretch-slim AS php-build-prereqs
COPY images/build/php /php
COPY build/__php-versions.sh /php/
RUN set -eux && chmod +x /php/*.sh && ls -la /php && /php/install-prereqs.sh

# Build PHP 5.6
FROM php-build-prereqs AS php56-build
RUN set -eux && . /php/__php-versions.sh && PHP_VERSION=$PHP56_VERSION GPG_KEYS=$PHP56_KEYS PHP_SHA256=$PHP56_TAR_SHA256 /php/build.sh

# Build PHP 7.0
FROM php-build-prereqs AS php70-build
RUN set -eux && . /php/__php-versions.sh && PHP_VERSION=$PHP70_VERSION GPG_KEYS=$PHP70_KEYS PHP_SHA256=$PHP70_TAR_SHA256 /php/build.sh

# Build PHP 7.2
FROM php-build-prereqs AS php72-build
RUN set -eux && . /php/__php-versions.sh && PHP_VERSION=$PHP72_VERSION GPG_KEYS=$PHP72_KEYS PHP_SHA256=$PHP72_TAR_SHA256 /php/build.sh

# Build PHP 7.3
FROM php-build-prereqs AS php73-build
RUN set -eux && . /php/__php-versions.sh && PHP_VERSION=$PHP73_VERSION GPG_KEYS=$PHP73_KEYS PHP_SHA256=$PHP73_TAR_SHA256 /php/build.sh
# Install Composer so that it can be copied to the final image later
RUN set -eux && . /php/__php-versions.sh && \
    mkdir /opt/php-composer && \
    export phpbin="/opt/php/$PHP73_VERSION/bin/php" && \
    $phpbin -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    $phpbin -r "if (hash_file('sha384', 'composer-setup.php') === '$COMPOSER_SHA384') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;" && \
    $phpbin composer-setup.php --version=$COMPOSER_VERSION --install-dir=/opt/php-composer && \
    $phpbin -r "unlink('composer-setup.php');"

# This stage is used only when building locally
FROM dotnet-install AS buildscriptbuilder
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Common /usr/oryx/src/Common
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG AGENTBUILD=${AGENTBUILD}
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN if [ -z "$AGENTBUILD" ]; then dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release  BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; fi

# This stage is only when building in devops agents
FROM main AS copybuildscriptsymbol
COPY symbols /opt/buildscriptgen/
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Common /usr/oryx/src/Common
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}

# This stage copies oryx binaries from specific build stage, for local/dev it copies
# from "buildscriptbuilder" and for devops it copies from "copybuildscriptsymbol"
FROM ${BUILDSCRIPT_SOURCE} AS buildscriptbinaries
ARG BUILDSCRIPT_SOURCE
WORKDIR /usr/oryx/src
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

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
# Grant read-write permissions to the nuget folder so that dotnet restore
# can write into it.
RUN chmod a+rw /var/nuget
RUN ln -s /opt/dotnet/lts/dotnet /usr/local/bin/dotnet

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt
COPY --from=node-install /links/ /usr/local/bin
COPY images/build/node-cache-packages.sh /tmp/
RUN /tmp/node-cache-packages.sh && rm /tmp/node-cache-packages.sh

# Copy PHP versions
COPY --from=php-build-prereqs /php/install-prereqs.sh /tmp/php/install-prereqs.sh
RUN /tmp/php/install-prereqs.sh
COPY --from=php56-build /opt /opt
COPY --from=php70-build /opt /opt
COPY --from=php72-build /opt /opt
COPY --from=php73-build /opt /opt

# Build script generator content
COPY --from=buildscriptbinaries /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /usr/local/bin/oryx

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}

ENTRYPOINT [ "benv" ]