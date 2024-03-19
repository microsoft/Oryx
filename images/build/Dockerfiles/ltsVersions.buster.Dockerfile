FROM oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-buster AS main

# Install basic build tools
# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        git \
        make \
        unzip \
        # The tools in this package are used when installing packages for Python
        build-essential \
        swig3.0 \
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
        jq \
        # By default pip is not available in the buildpacks image
        python-pip \
        python3-pip \
        # GIS libraries for GeoDjango (https://docs.djangoproject.com/en/3.2/ref/contrib/gis/install/geolibs/)
        binutils \
        libproj-dev \
        gdal-bin \
        libgdal-dev \
        # Adding additional python packages to support all optional python modules:
        # https://devguide.python.org/getting-started/setup-building/index.html#install-dependencies
        python3-dev \
        libffi-dev \
        gdb \
        lcov \
        pkg-config \
        libgdbm-dev \
        liblzma-dev \
        libreadline6-dev \
        lzma \
        lzma-dev \
        zlib1g-dev \
    && rm -rf /var/lib/apt/lists/* \
    && pip install pip --upgrade \
    && pip3 install pip --upgrade \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

# NOTE: Place only folders whose size does not extrememly effect the perf of building this image
# since this intermediate stage is copied to final stage.
# For example, if we put yarn-cache here it is going to impact perf since it more than 500MB
FROM main AS intermediate
COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /opt/tmp
COPY --from=oryxdevmcr.azurecr.io/private/oryx/buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
 
FROM main AS final
ARG AI_CONNECTION_STRING
ARG SDK_STORAGE_BASE_URL_VALUE

COPY --from=intermediate /opt /opt

# Docker has an issue with variable expansion when all are used in a single ENV command.
# For example here the $LASTNAME in the following example does not expand to JORDAN but instead is empty: 
#   ENV LASTNAME="JORDAN" \
#       NAME="MICHAEL $LASTNAME"
#
# Even though this adds a new docker layer we are doing this 
# because we want to avoid duplication (which is always error-prone)
ENV ORYX_PATHS="/opt/oryx:/opt/nodejs/lts/bin:/opt/dotnet/lts:/opt/python/latest/bin:/opt/php/lts/bin:/opt/php-composer:/opt/yarn/stable/bin:/opt/hugo/lts"

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    ORIGINAL_PATH="$PATH" \
    PATH="$ORYX_PATHS:$PATH" \
    NUGET_XMLDOC_MODE="skip" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1" \
    NUGET_PACKAGES="/var/nuget" \
    ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}" \
    ENABLE_DYNAMIC_INSTALL="true" \
    ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} \
    PYTHONIOENCODING="UTF-8" \
    DEBIAN_FLAVOR="buster"

RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
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
       $imagesDir/build/installDotNetCore.sh \
    && DOTNET_SDK_VER=$DOT_NET_CORE_31_SDK_VERSION \
       INSTALL_PACKAGES="true" \
       $imagesDir/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPackagesDir -type d -exec chmod 777 {} \; \
    && cd /opt/dotnet \
    && . $buildDir/__dotNetCoreSdkVersions.sh \
    && ln -s $DOT_NET_CORE_31_SDK_VERSION 3-lts \
    && ln -s 3-lts lts \
    # Install Hugo
    && $imagesDir/build/installHugo.sh \
    # Install Node
    && . $buildDir/__nodeVersions.sh \
    && $imagesDir/installPlatform.sh nodejs $NODE14_VERSION \
    && $imagesDir/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
    && ${imagesDir}/retry.sh "curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
    && ${imagesDir}/retry.sh "curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
    && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && mkdir -p /opt/yarn \
    && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
    && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
    && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && cd /opt/nodejs \
    && ln -s $NODE14_VERSION 14 \
    && ln -s 14 lts \
    && npm install -g lerna@4.0.0 \
    && cd /opt/yarn \
    && ln -s $YARN_VERSION stable \
    && ln -s $YARN_VERSION latest \
    && ln -s $YARN_VERSION $YARN_MINOR_VERSION \
    && ln -s $YARN_MINOR_VERSION $YARN_MAJOR_VERSION \
    # Install Python SDKs
    # Upgrade system python
    && pip install --upgrade cython \
    && pip3 install --upgrade cython \
    # https://github.com/microsoft/artifacts-keyring
    # provides auth for publishing or consuming Python packages to or from Azure Artifacts feeds.
    && pip install twine keyring artifacts-keyring \
    && . $buildDir/__pythonVersions.sh \
    && $imagesDir/installPlatform.sh python $PYTHON39_VERSION \
    && [ -d "/opt/python/$PYTHON39_VERSION" ] && echo /opt/python/$PYTHON39_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -s $PYTHON39_VERSION 3.9 \
    && ln -s $PYTHON39_VERSION latest \
    && ln -s $PYTHON39_VERSION stable \
    && ln -s 3.9 3 \
    && echo "value of DEBIAN_FLAVOR is ${DEBIAN_FLAVOR}" \
    # Install PHP pre-reqs
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses5 \
    && rm -r /var/lib/apt/lists/* \
    # Copy PHP versions
    && . $buildDir/__phpVersions.sh \
    && $imagesDir/installPlatform.sh php $PHP80_VERSION \
    && $imagesDir/installPlatform.sh php-composer $COMPOSER1_10_VERSION \
    && cd /opt/php \
    && ln -s 8.0 8 \
    && ln -s 8 lts \
    && cd /opt/php-composer \
    && ln -sfn 2.6.2 stable \
    && ln -sfn /opt/php-composer/stable/composer.phar /opt/php-composer/composer.phar \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        libargon2-0 \
        libonig-dev \
    && rm -rf /var/lib/apt/lists/* \
    && cp -f $imagesDir/build/benv.sh /opt/oryx/benv \
    && cp -f $imagesDir/build/logger.sh /opt/oryx/logger \
    && mkdir -p /usr/local/share/pip-cache/lib \
    && chmod -R 777 /usr/local/share/pip-cache \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "ltsversions" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype \
    # as per solution 2 https://stackoverflow.com/questions/65921037/nuget-restore-stopped-working-inside-docker-container
    && ${imagesDir}/retry.sh "curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083" \
    && update-ca-certificates \
    && echo "value of DEBIAN_FLAVOR is ${DEBIAN_FLAVOR}"    

ENTRYPOINT [ "benv" ]
