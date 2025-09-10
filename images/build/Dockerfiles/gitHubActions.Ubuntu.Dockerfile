ARG BASE_IMAGE

# =================== MAIN BASE IMAGE ====================
# This stage sets up the base Ubuntu image with essential build tools
FROM ${BASE_IMAGE} AS main
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR

COPY binaries /opt/buildscriptgen/
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript /opt/buildscriptgen/Microsoft.Oryx.BuildServer

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
        tk-dev \
        uuid-dev \
        #.NET Core related pre-requisites
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
        libgdiplus \
        # For .NET Core 1.1
        libuuid1 \
        libunwind8 \
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
        libargon2-1 \
        libonig-dev \
        libicu74 \
        libcurl4 \
        libssl3 \
        libyaml-dev \
        libxml2 \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses6 \
    && rm -rf /var/lib/apt/lists/* \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

# =================== INTERMEDIATE IMAGE ====================
# This stage installs Yarn, HUGO, and copies build scripts
FROM main AS intermediate

ARG IMAGES_DIR=/opt/tmp/images
ARG BUILD_DIR=/opt/tmp/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# Install HUGO
RUN ${IMAGES_DIR}/build/installHugo.sh

# Install Yarn
ARG YARN_VERSION
ARG YARN_MINOR_VERSION
ARG YARN_MAJOR_VERSION

COPY images/yarn-v$YARN_VERSION.tar.gz .
RUN set -ex \
 && yarnCacheFolder="/usr/local/share/yarn-cache" \
 && mkdir -p $yarnCacheFolder \
 && chmod 777 $yarnCacheFolder \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz

RUN set -ex \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION

RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

# =================== FINAL IMAGE ====================
# This stage creates the final image with all components
FROM main AS final
ARG SDK_STORAGE_BASE_URL_VALUE
ARG IMAGES_DIR="/opt/tmp/images"
ARG AI_CONNECTION_STRING

COPY --from=intermediate /opt /opt

# as per solution 2 https://stackoverflow.com/questions/65921037/nuget-restore-stopped-working-inside-docker-container
RUN ${IMAGES_DIR}/retry.sh "curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083" \
    && update-ca-certificates \
    && echo "value of OS_FLAVOR is ${OS_FLAVOR}"

# Final setup and configuration
RUN tmpDir="/opt/tmp" \
    && cp -f $tmpDir/images/build/benv.sh /opt/oryx/benv \
    && cp -f $tmpDir/images/build/logger.sh /opt/oryx/logger \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger \
    && mkdir -p /usr/local/share/pip-cache/lib \
    && chmod -R 777 /usr/local/share/pip-cache \
    # Grant read-write permissions to the nuget folder so that dotnet restore
    # can write into it.
    && mkdir -p /var/nuget \
    && chmod a+rw /var/nuget \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "githubactions" > /opt/oryx/.imagetype \
    && rm -rf ${tmpDir} \
    && echo "${UBUNTU}|${OS_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype


# Docker has an issue with variable expansion when all are used in a single ENV command.
# For example here the $LASTNAME in the following example does not expand to JORDAN but instead is empty: 
#   ENV LASTNAME="JORDAN" \
#       NAME="MICHAEL $LASTNAME"
#
# Even though this adds a new docker layer we are doing this 
# because we want to avoid duplication (which is always error-prone)
ENV ORYX_PATHS="/opt/oryx:/opt/yarn/stable/bin:/opt/hugo/lts"

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    ORIGINAL_PATH="$PATH" \
    PATH="$ORYX_PATHS:$PATH" \
    NUGET_XMLDOC_MODE="skip" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1" \
    NUGET_PACKAGES="/var/nuget" \
    ORYX_AI_CONNECTION_STRING="${AI_CONNECTION_STRING}" \
    ENABLE_DYNAMIC_INSTALL="true" \
    ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}"

ENTRYPOINT [ "benv" ]
