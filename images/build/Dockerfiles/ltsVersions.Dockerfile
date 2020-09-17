FROM githubrunners-buildpackdeps-stretch AS main

# Install basic build tools
# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
RUN LANG="C.UTF-8" \
    && apt-get update \
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
        libgdiplus \
        # By default pip is not available in the buildpacks image
        python-pip \
        python3-pip \
    && rm -rf /var/lib/apt/lists/* \
    && pip install pip --upgrade \
    && pip3 install pip --upgrade \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

# Install .NET Core
FROM main AS intermediate
COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
COPY --from=support-files-image-for-build /tmp/oryx/ /opt/tmp
ARG BUILD_DIR="/opt/tmp/build"
ARG IMAGES_DIR="/opt/tmp/images"
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
	NUGET_PACKAGES=/opt/nuget

RUN mkdir /opt/nuget

# Check https://www.microsoft.com/net/platform/support-policy for support policy of .NET Core versions
RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_21_SDK_VERSION \
    INSTALL_PACKAGES="true" \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh && \
    DOTNET_SDK_VER=$DOT_NET_CORE_31_SDK_VERSION \
    INSTALL_PACKAGES="true" \
    ${IMAGES_DIR}/build/installDotNetCore.sh

RUN set -ex \
    rm -rf /tmp/NuGetScratch \
    && find /opt/nuget -type d -exec chmod 777 {} \;

RUN set -ex \
 && cd /opt/dotnet \
 && . ${BUILD_DIR}/__dotNetCoreSdkVersions.sh \
 && ln -s $DOT_NET_CORE_31_SDK_VERSION lts \
 && ln -s lts/dotnet /usr/local/bin/dotnet

# Install Node.js, NPM, Yarn
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN ${IMAGES_DIR}/build/installHugo.sh
COPY build/__nodeVersions.sh /tmp/scripts
RUN cd ${IMAGES_DIR} \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ./installPlatform.sh nodejs $NODE8_VERSION \
 && ./installPlatform.sh nodejs $NODE10_VERSION \
 && ./installPlatform.sh nodejs $NODE12_VERSION
RUN ${IMAGES_DIR}/build/installNpm.sh
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ${IMAGES_DIR}/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $NODE8_VERSION /opt/nodejs/8 \
 && ln -s $NODE10_VERSION /opt/nodejs/10 \
 && ln -s $NODE12_VERSION /opt/nodejs/12 \
 && ln -s 12 /opt/nodejs/lts
RUN set -ex \
 && ln -s 6.9.0 /opt/npm/6.9 \
 && ln -s 6.9 /opt/npm/6 \
 && ln -s 6 /opt/npm/latest
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

FROM main AS final
ARG AI_KEY
ARG SDK_STORAGE_BASE_URL_VALUE
COPY --from=intermediate /opt /opt
RUN imagesDir="/opt/tmp/images" \
    && buildDir="/opt/tmp/build" \
    && mkdir -p /var/nuget \
    && cd /opt/nuget \
    && mv * /var/nuget \
    && cd /opt \
    && rm -rf /opt/nuget \
    # Grant read-write permissions to the nuget folder so that dotnet restore
    # can write into it.
    && chmod a+rw /var/nuget \
    # https://github.com/docker-library/python/issues/147
    && PYTHONIOENCODING="UTF-8" \
    # It's not clear whether these are needed at runtime...
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
    && rm -rf /var/lib/apt/lists/* \
    && cd $imagesDir \
    && . $buildDir/__pythonVersions.sh \
    && ./installPlatform.sh python $PYTHON37_VERSION \
    && ./installPlatform.sh python $PYTHON38_VERSION \
    && . $buildDir/__pythonVersions.sh \
    && set -ex \
    && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && . $buildDir/__pythonVersions.sh && set -ex \
    && ln -s $PYTHON37_VERSION /opt/python/3.7 \
    && ln -s $PYTHON38_VERSION /opt/python/3.8 \
    && ln -s $PYTHON38_VERSION /opt/python/latest \
    && ln -s $PYTHON38_VERSION /opt/python/stable \
    && ln -s 3.8 /opt/python/3 \
    # Install PHP pre-reqs
    && $imagesDir/build/php/prereqs/installPrereqs.sh \
    # Copy PHP versions
    && cd $imagesDir \
    && . $buildDir/__phpVersions.sh \
    && ./installPlatform.sh php $PHP73_VERSION \
    && ./installPlatform.sh php-composer $COMPOSER_VERSION \
    && ln -s /opt/php/7.3 /opt/php/7 \
    && ln -s /opt/php/7 /opt/php/lts \
    && cd /opt/php-composer \
    && ln -sfn 1.9.3 stable \
    && ln -sfn /opt/php-composer/stable/composer.phar /opt/php-composer/composer.phar \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        libargon2-0 \
        libonig-dev \
    && rm -rf /var/lib/apt/lists/* \
    && cp -f $imagesDir/build/benv.sh /opt/oryx/benv \
    && mkdir -p /usr/local/share/pip-cache/lib \
    && chmod -R 777 /usr/local/share/pip-cache \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && rm -f /etc/apt/sources.list.d/buster.list \
    && rm -rf /opt/tmp

# Docker has an issue with variable expansion when all are used in a single ENV command.
# For example here the $LASTNAME in the following example does not expand to JORDAN but instead is empty: 
#   ENV LASTNAME="JORDAN" \
#       NAME="MICHAEL $LASTNAME"
#
# Even though this adds a new docker layer we are doing this 
# because we want to avoid duplication (which is always error-prone)
ENV ORYX_PATHS="/opt/oryx:/opt/nodejs/lts/bin:/opt/dotnet/lts:/opt/python/latest/bin:/opt/php/lts/bin:/opt/php-composer:/opt/yarn/stable/bin:/opt/hugo/lts"

ENV LANG="C.UTF-8" \
    ORIGINAL_PATH="$PATH" \
    PATH="$ORYX_PATHS:$PATH" \
    NUGET_XMLDOC_MODE="skip" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1" \
    NUGET_PACKAGES="/var/nuget" \
    ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}" \
    ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY} \
    PYTHONIOENCODING="UTF-8"

ENTRYPOINT [ "benv" ]
