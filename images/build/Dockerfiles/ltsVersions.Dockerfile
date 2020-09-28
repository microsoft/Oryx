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
        # For .NET Core 1.1
        libcurl3 \
        libuuid1 \
        libunwind8 \
        jq \
    && rm -rf /var/lib/apt/lists/* \
    && pip install pip --upgrade \
    && pip3 install pip --upgrade \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

# NOTE: Place only folders whose size does not extrememly effect the perf of building this image
# For example, if we put yarn-cache here it is going to impact perf since it more than 500MB
FROM main AS intermediate
COPY --from=support-files-image-for-build /tmp/oryx/ /opt/tmp
COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
RUN du -hs /opt/tmp
 
FROM main AS final
ARG AI_KEY
ARG SDK_STORAGE_BASE_URL_VALUE
COPY --from=intermediate /opt /opt
RUN set -ex \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    # https://github.com/docker-library/python/issues/147
    && PYTHONIOENCODING="UTF-8" \
    # It's not clear whether these are needed at runtime...
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
    && rm -rf /var/lib/apt/lists/* \
    # Install .NET Core SDKs
    && nugetPackagesDir="/var/nuget" \
    && mkdir -p $nugetPackagesDir \
    # Grant read-write permissions to the nuget folder so that dotnet restore
    # can write into it.
    && chmod a+rw $nugetPackagesDir \
    && DOTNET_RUNNING_IN_CONTAINER=true \
    && DOTNET_USE_POLLING_FILE_WATCHER=true \
	&& NUGET_XMLDOC_MODE=skip \
    && DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
	&& NUGET_PACKAGES="$nugetPackagesDir" \
    && . $buildDir/__dotNetCoreSdkVersions.sh \
    && DOTNET_SDK_VER=$DOT_NET_CORE_21_SDK_VERSION \
       INSTALL_PACKAGES="true" \
       $imagesDir/build/installDotNetCore.sh \
    && DOTNET_SDK_VER=$DOT_NET_CORE_31_SDK_VERSION \
       INSTALL_PACKAGES="true" \
       $imagesDir/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPackagesDir -type d -exec chmod 777 {} \; \
    # Install Hugo
    && $imagesDir/build/installHugo.sh \
    # Install Node
    && . $buildDir/__nodeVersions.sh \
    && cd $imagesDir \
    && ./installPlatform.sh nodejs $NODE8_VERSION \
    && ./installPlatform.sh nodejs $NODE10_VERSION \
    && ./installPlatform.sh nodejs $NODE12_VERSION \
    && $imagesDir/build/installNpm.sh \
    && $imagesDir/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
    && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
    && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
    && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && mkdir -p /opt/yarn \
    && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
    && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
    && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && ln -s $NODE8_VERSION /opt/nodejs/8 \
    && ln -s $NODE10_VERSION /opt/nodejs/10 \
    && ln -s $NODE12_VERSION /opt/nodejs/12 \
    && ln -s 12 /opt/nodejs/lts \
    && ln -s 6.9.0 /opt/npm/6.9 \
    && ln -s 6.9 /opt/npm/6 \
    && ln -s 6 /opt/npm/latest \
    && ln -s $YARN_VERSION /opt/yarn/stable \
    && ln -s $YARN_VERSION /opt/yarn/latest \
    && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
    && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION \
    && cd $imagesDir \
    && . $buildDir/__pythonVersions.sh \
    && ./installPlatform.sh python $PYTHON37_VERSION \
    && ./installPlatform.sh python $PYTHON38_VERSION \
    && [ -d "/opt/python/$PYTHON37_VERSION" ] && echo /opt/python/$PYTHON37_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && ln -s $PYTHON37_VERSION /opt/python/3.7 \
    && ln -s $PYTHON38_VERSION /opt/python/3.8 \
    && ln -s $PYTHON38_VERSION /opt/python/latest \
    && ln -s $PYTHON38_VERSION /opt/python/stable \
    && ln -s 3.8 /opt/python/3 \
    && pip install --upgrade cython \
    && pip3 install --upgrade cython \
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
    && rm -f /etc/apt/sources.list.d/buster.list

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
    ENABLE_DYNAMIC_INSTALL="true" \
    ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY} \
    PYTHONIOENCODING="UTF-8"

ENTRYPOINT [ "benv" ]
