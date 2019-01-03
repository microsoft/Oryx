FROM buildpack-deps:stable AS main

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
 && rm -rf /var/lib/apt/lists/*

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions
RUN set -ex \
 && DOTNET_DIR=/opt/dotnet \
 && mkdir -p $DOTNET_DIR \
 # Install .NET Core SDK 2.2.100
 && DOTNET_SDK_VER=2.2.100 \
 && DOTNET_SDK_URL=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VER/dotnet-sdk-$DOTNET_SDK_VER-linux-x64.tar.gz \
 && DOTNET_SDK_SHA=6bde1d0f186f068b1300d5a67e8aba56ff271b940bc0782c3a254dc0f67e7167d2fca12fc51eb3319d4ab4a91cbe5639e5104e9e0036adb8a27ca5711453a1c3 \
 && curl -SL $DOTNET_SDK_URL --output dotnet.tar.gz \
 && echo "$DOTNET_SDK_SHA dotnet.tar.gz" | sha512sum -c - \
 && tar -xzf dotnet.tar.gz -C $DOTNET_DIR \
 && rm dotnet.tar.gz \
 && ln -s $DOTNET_DIR/dotnet /usr/bin/dotnet

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
	NUGET_XMLDOC_MODE=skip \
    ASPNETCORE_URLS=http://+:80 \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget

# Trigger the population of the local package cache
# We install the 1.x web templates just so we can create and restore 1.0 and 1.1 MVC apps here
# See https://github.com/aspnet/Tooling/blob/master/missing-template.md
# Note that after populating the cache we need to chmod 777 every directory underneath
# so the Kudu user can access them; this can be removed if we run Kudu as root later.
RUN mkdir /var/nuget \
 && chmod a+rw /var/nuget \
 && mkdir warmup \
 && cd warmup \
 && dotnet new mvc \
 && dotnet restore \
 && cd .. \
 && rm -rf warmup \
 && rm -rf /tmp/NuGetScratch \
 && find /var/nuget -type d -exec chmod 777 {} \;

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
 && ~/n/bin/n -d 9.4.0 \
 && ~/n/bin/n -d 10.1.0 \
 && ~/n/bin/n -d 10.10.0 \
 && ~/n/bin/n -d 10.14.1 \
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
RUN set -ex \
 && YARN_VERSION=1.12.3 \
 && GPG_KEY=6A010C5166006599AA17F08146C2130DFD2497F5 \
 && gpg --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys "$GPG_KEY" || \
    gpg --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys "$GPG_KEY" || \
    gpg --keyserver hkp://pgp.mit.edu:80 --recv-keys "$GPG_KEY" \
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
 && ln -s 8.12 /opt/nodejs/8 \
 && ln -s 9.4.0 /opt/nodejs/9.4 \
 && ln -s 9.4 /opt/nodejs/9 \
 && ln -s 10.1.0 /opt/nodejs/10.1 \
 && ln -s 10.10.0 /opt/nodejs/10.10 \
 && ln -s 10.14.1 /opt/nodejs/lts \
 && ln -s 10.14.1 /opt/nodejs/10.14 \
 && ln -s 10.14 /opt/nodejs/10
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
 && ln -s 1.12.3 /opt/yarn/stable \
 && ln -s 1.12.3 /opt/yarn/latest \
 && ln -s 1.12.3 /opt/yarn/1.12 \
 && ln -s 1.12 /opt/yarn/1
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

# Install Python build prerequisites
FROM buildpack-deps:stable AS python-build-prereqs
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      tk-dev \
      uuid-dev

# Build Python 2.7
FROM python-build-prereqs AS python2.7-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=2.7.15
ENV GPG_KEY C01E1CAD5EA2C4F0B8E3571504C367C218ADD4FF
ENV PIP_VERSION=18.1
COPY images/build/python-build.sh /
RUN chmod +x /python-build.sh
RUN /python-build.sh

# Build Python 3.5
FROM python-build-prereqs AS python3.5-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.5.6
ENV GPG_KEY 97FC712E4C024BBEA48A61ED3A5CA953F73C700D
ENV PIP_VERSION=18.1
COPY images/build/python-build.sh /
RUN chmod +x /python-build.sh
RUN /python-build.sh

# Build Python 3.6
FROM python-build-prereqs AS python3.6-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.6.7
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.1
COPY images/build/python-build.sh /
RUN chmod +x /python-build.sh
RUN /python-build.sh

# Build Python 3.7
FROM python-build-prereqs AS python3.7-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.7.1
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.1
COPY images/build/python-build.sh /
RUN chmod +x /python-build.sh
RUN /python-build.sh

FROM main AS python
# It's not clear whether these are needed at runtime...
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8
COPY --from=python2.7-build /opt /opt
COPY --from=python3.5-build /opt /opt
COPY --from=python3.6-build /opt /opt
COPY --from=python3.7-build /opt /opt
RUN set -ex \
 && echo /opt/python/2.7.15/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.5.6/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.6.7/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.7.1/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN set -ex \
 && ln -s 2.7.15 /opt/python/2.7 \
 && ln -s 2.7 /opt/python/2 \
 && ln -s 3.5.6 /opt/python/3.5 \
 && ln -s 3.6.7 /opt/python/3.6 \
 && ln -s 3.7.1 /opt/python/latest \
 && ln -s 3.7.1 /opt/python/3.7 \
 && ln -s 3.7 /opt/python/3
RUN set -ex \
 && cd /usr/local/bin \
 && cp -sn /opt/python/2/bin/* . \
 && cp -sn /opt/python/3/bin/* .

FROM dotnet-install AS buildscriptbuilder
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Oryx.Common.Utilities /usr/oryx/src/Oryx.Common.Utilities
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

FROM python AS final
WORKDIR /

# Copy .NET Core related content
ENV NUGET_XMLDOC_MODE=skip \
	DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	NUGET_PACKAGES=/var/nuget
COPY --from=dotnet-install /opt/dotnet /opt/dotnet
COPY --from=dotnet-install /var/nuget /var/nuget
RUN ln -s /opt/dotnet/dotnet /usr/local/bin/dotnet

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt
COPY --from=node-install /links/ /usr/local/bin

# Build script generator content
COPY --from=buildscriptbuilder /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /usr/local/bin/oryx

COPY images/build/benv.sh /usr/local/bin/benv
RUN chmod +x /usr/local/bin/benv

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}

ENTRYPOINT [ "benv" ]