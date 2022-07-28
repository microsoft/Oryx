ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-${DEBIAN_FLAVOR} AS main
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
# Install basic build tools
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
    && rm -rf /var/lib/apt/lists/* \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

RUN if [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu67 \
            libcurl4 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* \
        && curl -LO http://security.debian.org/debian-security/pool/updates/main/libx/libxml2/libxml2_2.9.10+dfsg-6.7+deb11u2_amd64.deb \
        && dpkg -i libxml2_2.9.10+dfsg-6.7+deb11u2_amd64.deb \
        && rm libxml2_2.9.10+dfsg-6.7+deb11u2_amd64.deb ; \
    elif [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu63 \
            libcurl4 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* ; \
    else \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libcurl3 \
            libicu57 \
            liblttng-ust0 \
            libssl1.0.2 \
        && rm -rf /var/lib/apt/lists/* ; \
    fi

# Install Yarn, HUGO
FROM main AS intermediate
COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /opt/tmp
COPY --from=oryxdevmcr.azurecr.io/private/oryx/buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
ARG BUILD_DIR="/opt/tmp/build"
ARG IMAGES_DIR="/opt/tmp/images" 
RUN ${IMAGES_DIR}/build/installHugo.sh
RUN set -ex \
 && yarnCacheFolder="/usr/local/share/yarn-cache" \
 && mkdir -p $yarnCacheFolder \
 && chmod 777 $yarnCacheFolder \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ${IMAGES_DIR}/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
 && ${IMAGES_DIR}/retry.sh "curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && ${IMAGES_DIR}/retry.sh "curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

FROM main AS final
ARG SDK_STORAGE_BASE_URL_VALUE
ARG TEMP_ORYX_PATHS
ARG BUILD_DIR="/opt/tmp/build"
ARG IMAGES_DIR="/opt/tmp/images"
ARG AI_KEY

COPY --from=intermediate /opt /opt

# as per solution 2 https://stackoverflow.com/questions/65921037/nuget-restore-stopped-working-inside-docker-container
RUN ${IMAGES_DIR}/retry.sh "curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083 && update-ca-certificates" \
    && echo "value of DEBIAN_FLAVOR is ${DEBIAN_FLAVOR}"

# Install PHP pre-reqs	# Install PHP pre-reqs
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ] || [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
    apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses5 \
    --no-install-recommends && rm -r /var/lib/apt/lists/* ; \
    else \
        .${IMAGES_DIR}/build/php/prereqs/installPrereqs.sh ; \
    fi 

# Temporary: Install Python 3.11 for bullseye
RUN if [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
    export PYTHON_VERSION="3.11.04b" \
    && apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        build-essential \ 
        tk-dev \
        uuid-dev \
        libgeos-dev \
    && cp ${IMAGES_DIR}/receiveGpgKeys.sh /tmp \
    && ${BUILD_DIR}/buildPythonSdkByVersion.sh 3.11.0b4 \
    && set -ex \
    && cd /opt/python/ \
    && ln -s 3.11.0b4 3.11 \
    && ln -s 3.11 3 \
    && echo /opt/python/3.11.04b/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python/3.11.04b/bin \
    && ln -nsf idle3 idle \
    && ln -nsf pydoc3 pydoc \
    && ln -nsf python3-config python-config \
    && rm -rf /var/lib/apt/lists/* ; \
fi

# Temporary: Install node 18.7.0 for bullseye
RUN if [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
    apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        make \
        jq \
    && curl -sL https://git.io/n-install | bash -s -- -ny - \
    && set -ex \
    && ~/n/bin/n -d 18.7.0 \
    && mkdir -p /opt/node \
    && cp -r /usr/local/n/versions/node/18.7.0 /opt/node \
    && rm -rf /usr/local/n ~/n \
    && rm -r /var/lib/apt/lists/* ; \
fi

# Temporary: Install PHP 8.1.6 for bullseye
RUN if [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
    . ${BUILD_DIR}/__phpVersions.sh \
    && ${IMAGES_DIR}/build/php/prereqs/installPrereqs.sh \
    && ${IMAGES_DIR}/installPlatform.sh php $PHP81_VERSION \
    && cd /opt/php \
    && ln -s $PHP81_VERSION 8 \
    && ln -s $PHP81_VERSION lts ; \
fi

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
    # Install PHP pre-reqs
    #&& $tmpDir/images/build/php/prereqs/installPrereqs.sh \
    # NOTE: do not include the following lines in prereq installation script as
    # doing so is causing different version of libargon library being installed
    # causing php-composer to fail
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        libargon2-0 \
        libonig-dev \
    && rm -rf /var/lib/apt/lists/* \
    && rm -f /etc/apt/sources.list.d/buster.list \
    && echo "githubactions" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

# Docker has an issue with variable expansion when all are used in a single ENV command.
# For example here the $LASTNAME in the following example does not expand to JORDAN but instead is empty: 
#   ENV LASTNAME="JORDAN" \
#       NAME="MICHAEL $LASTNAME"
#
# Even though this adds a new docker layer we are doing this 
# because we want to avoid duplication (which is always error-prone)
ENV ORYX_PATHS="${TEMP_ORYX_PATHS}/opt/oryx:/opt/yarn/stable/bin:/opt/hugo/lts"

ENV LANG="C.UTF-8" \
    ORIGINAL_PATH="$PATH" \
    PATH="$ORYX_PATHS:$PATH" \
    NUGET_XMLDOC_MODE="skip" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1" \
    NUGET_PACKAGES="/var/nuget" \
    ORYX_AI_INSTRUMENTATION_KEY="${AI_KEY}" \
    ENABLE_DYNAMIC_INSTALL="true" \
    ORYX_SDK_STORAGE_BASE_URL="${SDK_STORAGE_BASE_URL_VALUE}"

ENTRYPOINT [ "benv" ]
