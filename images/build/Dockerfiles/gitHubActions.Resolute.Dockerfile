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
        moreutils \
        rsync \
        zip \
        tk-dev \
        uuid-dev \
        #.NET Core related pre-requisites
        libc6 \
        libgcc-s1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
        libgdiplus \
        # For .NET Core 
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
        libxml2-16 \
        libreadline-dev \
        lzma \
        zlib1g-dev \
        libargon2-1 \
        libonig-dev \
        libicu78 \
        libcurl4t64 \
        libssl3t64 \
        libyaml-dev \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses6 \
        zstd \
        # From build container
        libssl-dev \
        gawk \
        g++ \
        gcc \
        libc6-dev \
        libreadline-dev \
        libncurses5-dev \
        libxslt-dev \
        libxml2-dev \
        wget \
        vim \
        tree \
    && rm -rf /var/lib/apt/lists/* \

# =================== FINAL IMAGE ====================
# This stage creates the final image with all components
FROM main AS final
ARG TEMP_DIR=/opt/tmp/
ARG AI_CONNECTION_STRING

COPY images/retry.sh ${TEMP_DIR}
COPY images/build/benv.sh  ${TEMP_DIR}
COPY images/build/logger.sh  ${TEMP_DIR}


RUN tmpDir="/opt/tmp" \
    && chmod +x $tmpDir/retry.sh \
    && $tmpDir/retry.sh "curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083" \
    && update-ca-certificates \
    && echo "value of OS_FLAVOR is ${OS_FLAVOR}" \
    && mkdir -p /opt/oryx \
    && cp -f $tmpDir/benv.sh /opt/oryx/benv \
    && cp -f $tmpDir/logger.sh /opt/oryx/logger \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger \
    && mkdir -p /usr/local/share/pip-cache/lib \
    && chmod -R 777 /usr/local/share/pip-cache \
    && mkdir -p /usr/local/share/uv-pip-cache \
    && chmod -R 777 /usr/local/share/uv-pip-cache \
    && mkdir -p /var/nuget \
    && chmod a+rw /var/nuget \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "githubactions" > /opt/oryx/.imagetype \
    && echo "UBUNTU|${OS_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype \
    && rm -rf /opt/tmp

    
# Resolute based Github Action Image to use ACR to fetch SDKs instead of Storage 
ARG ORYX_ENABLE_ACR_SDK_PROVIDER
ARG ORYX_ACR_SDK_REGISTRY_URL
ARG ORYX_ACR_SDK_REPOSITORY_PREFIX
ARG ORYX_DISABLE_CDN_SDK_PROVIDER

ENV ORYX_PATHS="/opt/oryx"

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
    ORYX_ENABLE_ACR_SDK_PROVIDER="${ORYX_ENABLE_ACR_SDK_PROVIDER}" \
    ORYX_ACR_SDK_REGISTRY_URL="${ORYX_ACR_SDK_REGISTRY_URL}" \
    ORYX_ACR_SDK_REPOSITORY_PREFIX="${ORYX_ACR_SDK_REPOSITORY_PREFIX}" \
    ORYX_DISABLE_CDN_SDK_PROVIDER="${ORYX_DISABLE_CDN_SDK_PROVIDER}"

ENTRYPOINT [ "benv" ]
