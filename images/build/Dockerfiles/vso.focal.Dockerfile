FROM oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-focal AS main

# Install basic build tools
# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
RUN apt-get update \
    && apt-get upgrade -y \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
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
        python-pip-whl \
        python3-pip \
        #.NET Core related pre-requisites
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libncurses5 \
        liblttng-ust0 \
        libssl-dev \
        libstdc++6 \
        zlib1g \
        libuuid1 \
        libunwind8 \
        sqlite3 \
        libsqlite3-dev \
        software-properties-common \
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
    # This is the folder containing 'links' to benv and build script generator
    && apt-get update \
    && apt-get upgrade -y \
    && add-apt-repository universe \
    && apt-get install -y --no-install-recommends python2 \
    && rm -rf /var/lib/apt/lists/* \
    # 'get-pip.py' has been moved to ' https://bootstrap.pypa.io/pip/2.7/get-pip.py' from 'https://bootstrap.pypa.io/2.7/get-pip.py'
    && curl  https://bootstrap.pypa.io/pip/2.7/get-pip.py --output get-pip.py \
    && python2 get-pip.py \
    && pip install pip --upgrade \
    && pip3 install pip --upgrade \
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

# add an environment variable to determine debian_flavor
# to correctly download platform sdk during platform installation
ENV DEBIAN_FLAVOR="focal-scm"
# Set sdk storage base url
ENV ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}"
COPY --from=intermediate /opt /opt

# Docker has an issue with variable expansion when all are used in a single ENV command.
# For example here the $LASTNAME in the following example does not expand to JORDAN but instead is empty: 
#   ENV LASTNAME="JORDAN" \
#       NAME="MICHAEL $LASTNAME"
#
# Even though this adds a new docker layer we are doing this 
# because we want to avoid duplication (which is always error-prone)

RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN="$(cat /run/secrets/oryx_sdk_storage_account_access_token)" \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    # https://github.com/docker-library/python/issues/147
    && PYTHONIOENCODING="UTF-8" \    
    && apt-get update \
    && apt-get upgrade -y \
    # It's not clear whether these are needed at runtime...
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
    && DOTNET_SDK_VER=$DOT_NET_CORE_31_SDK_VERSION \
       INSTALL_PACKAGES="true" \
       $imagesDir/build/installDotNetCore.sh \
    && DOTNET_SDK_VER=$DOT_NET_60_SDK_VERSION \
       INSTALL_PACKAGES="true" \
       $imagesDir/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPackagesDir -type d -exec chmod 777 {} \; \
    && cd /opt/dotnet \
    && . $buildDir/__dotNetCoreSdkVersions.sh \
    && ln -s $DOT_NET_CORE_31_SDK_VERSION 3-lts \
    && ln -s $DOT_NET_60_SDK_VERSION 6-lts \
    && ln -s $DOT_NET_60_SDK_VERSION lts \
    # Install Hugo
    && mkdir -p /home/codespace/.hugo \
    && $imagesDir/build/installHugo.sh \
    # Install Node
    && mkdir -p /home/codespace/nvm \
    && . $buildDir/__nodeVersions.sh \
    && $imagesDir/installPlatform.sh nodejs $NODE14_VERSION \
    && $imagesDir/installPlatform.sh nodejs $NODE16_VERSION \
    && $imagesDir/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
    && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
    && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
    && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && mkdir -p /opt/yarn \
    && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
    && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
    && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
    && cd /opt/nodejs \
    && ln -s $NODE14_VERSION 14 \
    && ln -s $NODE16_VERSION 16 \
    && ln -s $NODE16_VERSION lts \
    && ln -sfn /opt/nodejs/$NODE16_VERSION /home/codespace/nvm/current \
    && cd /opt/yarn \
    && ln -s $YARN_VERSION stable \
    && ln -s $YARN_VERSION latest \
    && ln -s $YARN_VERSION $YARN_MINOR_VERSION \
    && ln -s $YARN_MINOR_VERSION $YARN_MAJOR_VERSION \
    # Install Python SDKs
    # Upgrade system python
    && mkdir -p /home/codespace/.python \
    && pip install --upgrade cython \
    && pip3 install --upgrade cython \
    && . $buildDir/__pythonVersions.sh \
    && $imagesDir/installPlatform.sh python $PYTHON39_VERSION \
    && $imagesDir/installPlatform.sh python $PYTHON310_VERSION \
    && [ -d "/opt/python/$PYTHON39_VERSION" ] && echo /opt/python/$PYTHON39_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON310_VERSION" ] && echo /opt/python/$PYTHON310_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -s $PYTHON39_VERSION 3.9 \
    && ln -s $PYTHON310_VERSION 3.10 \
    && ln -s $PYTHON310_VERSION latest \
    && ln -s $PYTHON310_VERSION stable \
    && ln -s $PYTHON310_VERSION 3 \
    && ln -sfn /opt/python/$PYTHON310_VERSION /home/codespace/.python/current \
    # Install PHP pre-reqs
    && $imagesDir/build/php/prereqs/installPrereqs.sh \
    && mkdir -p /home/codespace/.php \
    # Copy PHP versions
    && . $buildDir/__phpVersions.sh \
    && $imagesDir/installPlatform.sh php $PHP80_VERSION \
    && $imagesDir/installPlatform.sh php $PHP81_VERSION \
    && $imagesDir/installPlatform.sh php-composer $COMPOSER2_3_VERSION \
    && cd /opt/php \
    && ln -s $PHP81_VERSION 8 \
    && ln -s $PHP81_VERSION lts \
    && ln -sfn /opt/php/$PHP81_VERSION /home/codespace/.php/current \
    && cd /opt/php-composer \
    && ln -sfn $COMPOSER2_3_VERSION stable \
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
    && rm -f /etc/apt/sources.list.d/buster.list \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN=""

ENV ORYX_PATHS="/opt/oryx:/opt/nodejs/lts/bin:/opt/dotnet/lts:/opt/python/latest/bin:/opt/php/lts/bin:/opt/php-composer:/opt/yarn/stable/bin:/opt/hugo/lts::/opt/java/lts/bin:/opt/maven/lts/bin:/opt/ruby/lts/bin"

ENV ORYX_PREFER_USER_INSTALLED_SDKS=true \
    ORIGINAL_PATH="$PATH" \
    PATH="$ORYX_PATHS:$PATH" \
    CONDA_SCRIPT="/opt/conda/etc/profile.d/conda.sh" \
    RUBY_HOME="/opt/ruby/lts" \
    JAVA_HOME="/opt/java/lts" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt"

# Now adding remaining of VSO platform features
RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN="$(cat /run/secrets/oryx_sdk_storage_account_access_token)" \
    && buildDir="/opt/tmp/build" \
    && imagesDir="/opt/tmp/images" \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        apt-transport-https \
        nano \
    && rm -rf /var/lib/apt/lists/* \
    && curl https://repo.anaconda.com/pkgs/misc/gpgkeys/anaconda.asc | gpg --dearmor > conda.gpg \
    && install -o root -g root -m 644 conda.gpg /usr/share/keyrings/conda-archive-keyring.gpg \
    && gpg --keyring /usr/share/keyrings/conda-archive-keyring.gpg --no-default-keyring --fingerprint 34161F5BF5EB1D4BFBBB8F0A8AEB4F8B29D82806 \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/conda-archive-keyring.gpg] https://repo.anaconda.com/pkgs/misc/debrepo/conda stable main" > /etc/apt/sources.list.d/conda.list \
    && . $buildDir/__condaConstants.sh \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        conda=${CONDA_VERSION} \
    && rm -rf /var/lib/apt/lists/* \
    && echo $$CONDA_SCRIPT \
    &&. $CONDA_SCRIPT \
    && conda config --add channels conda-forge \
    && conda config --set channel_priority strict \
    && conda config --set env_prompt '({name})' \
    && echo "source ${CONDA_SCRIPT}" >> ~/.bashrc \
    && condaDir="/opt/oryx/conda" \
    && mkdir -p "$condaDir" \
    && cd $imagesDir/build/python/conda \
    && cp -rf * "$condaDir" \
    && cd $imagesDir \
    && mkdir -p /home/codespace/.ruby \
    && . $buildDir/__rubyVersions.sh \
    && ./installPlatform.sh ruby $RUBY30_VERSION \
    && ./installPlatform.sh ruby $RUBY31_VERSION \
    && cd /opt/ruby \
    && ln -s $RUBY30_VERSION /opt/ruby/lts \
    && ln -sfn /opt/ruby/$RUBY30_VERSION /home/codespace/.ruby/current \
    && cd $imagesDir \
    && mkdir -p /home/codespace/java \
    && . $buildDir/__javaVersions.sh \
    && ./installPlatform.sh java $JAVA_VERSION \
    && ./installPlatform.sh java $JAVA_VERSION11 \
    && ./installPlatform.sh maven $MAVEN_VERSION \
    && cd /opt/java \
    && ln -s $JAVA_VERSION lts \
    && ln -sfn /opt/java/$JAVA_VERSION /home/codespace/java/current \
    && cd /opt/maven \
    && ln -s $MAVEN_VERSION lts \
    && mkdir -p /home/codespace/.maven/current \
    && ln -sfn /opt/maven/$MAVEN_VERSION /home/codespace/.maven/current \
    && npm install -g lerna@4.0.0 \
    && PATH="$PATH:/opt/php/lts/bin" \
    && wget http://pear.php.net/go-pear.phar \
    && php go-pear.phar \
    && pecl install -f libsodium \
    && echo "vso-focal" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype\
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN=""

# install few more tools for VSO
RUN gem install bundler rake ruby-debug-ide debase jekyll
RUN  yes | pecl install xdebug \
    && export PHP_LOCATION=$(dirname $(dirname $(which php))) \
    && echo "zend_extension=$(find ${PHP_LOCATION}/lib/php/extensions/ -name xdebug.so)" > ${PHP_LOCATION}/ini/conf.d/xdebug.ini \
    && echo "xdebug.mode = debug" >> ${PHP_LOCATION}/ini/conf.d/xdebug.ini \
    && echo "xdebug.start_with_request = yes" >> ${PHP_LOCATION}/ini/conf.d/xdebug.ini \
    && echo "xdebug.client_port = 9000" >> ${PHP_LOCATION}/ini/conf.d/xdebug.ini
 
RUN ./opt/tmp/build/createSymlinksForDotnet.sh

RUN groupadd -g 1000 codespace
RUN useradd -u 1000 -g codespace codespace
RUN chown -R codespace:codespace /home/codespace/
RUN chown -R codespace:codespace /opt/

ENV NUGET_XMLDOC_MODE="skip" \
    # VSO requires user installed tools to be preferred over Oryx installed tools
    PATH="$ORIGINAL_PATH:$ORYX_PATHS" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1" \
    NUGET_PACKAGES="/var/nuget" \
    ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}" \
    ENABLE_DYNAMIC_INSTALL="true" \
    ORYX_PREFER_USER_INSTALLED_SDKS=true \
    ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

ENTRYPOINT [ "benv" ]