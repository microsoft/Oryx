ARG BASE_IMAGE

FROM mcr.microsoft.com/dotnet/sdk:7.0 as buildscriptgenerator

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}

WORKDIR /usr/oryx
COPY build build
# This statement copies signed oryx binaries from during agent build.
# For local/dev contents of blank/empty directory named binaries are getting copied
COPY binaries /opt/buildscriptgen/
COPY src src
COPY build/FinalPublicKey.snk build/

RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript
RUN chmod a+x /opt/buildscriptgen/Microsoft.Oryx.BuildServer

FROM ${BASE_IMAGE} AS main
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

# stretch was removed from security.debian.org and deb.debian.org, so update the sources to point to the archived mirror
RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
    fi

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
    && rm -rf /var/lib/apt/lists/* \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

RUN if [ "${DEBIAN_FLAVOR}" = "bookworm" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu72 \
            libcurl4 \
            libssl3 \
            libyaml-dev \
            libxml2 \
        && rm -rf /var/lib/apt/lists/* ; \
    elif [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu67 \
            libcurl4 \
            libssl1.1 \
            libyaml-dev \
            libxml2 \
            # Adding lxml depended packages to avoid build failures
            # https://lxml.de/installation.html#requirements
            libxml2-dev \
            libxslt-dev \
            python3-dev \
            python3-setuptools \
            python3-wheel \
        && rm -rf /var/lib/apt/lists/* ; \
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

ARG IMAGES_DIR=/opt/tmp/images
ARG BUILD_DIR=/opt/tmp/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/

RUN ${IMAGES_DIR}/build/installHugo.sh

COPY images/yarn-v1.22.15.tar.gz .
RUN set -ex \
 && yarnCacheFolder="/usr/local/share/yarn-cache" \
 && mkdir -p $yarnCacheFolder \
 && chmod 777 $yarnCacheFolder \
#  && . ${BUILD_DIR}/__nodeVersions.sh \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v1.22.15.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v1.22.15 /opt/yarn/1.22.15 \
 && rm yarn-v1.22.15.tar.gz

 ARG YARN_VERSION
 ARG YARN_MINOR_VERSION
 ARG YARN_MAJOR_VERSION

RUN set -ex \
#  && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

FROM main AS final
ARG SDK_STORAGE_BASE_URL_VALUE
ARG IMAGES_DIR="/opt/tmp/images"
ARG AI_CONNECTION_STRING

COPY --from=intermediate /opt /opt

# as per solution 2 https://stackoverflow.com/questions/65921037/nuget-restore-stopped-working-inside-docker-container
RUN ${IMAGES_DIR}/retry.sh "curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083" \
    && update-ca-certificates \
    && echo "value of DEBIAN_FLAVOR is ${DEBIAN_FLAVOR}"

# Install PHP pre-reqs	# Install PHP pre-reqs
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ] || [ "${DEBIAN_FLAVOR}" = "bullseye" ] || [ "${DEBIAN_FLAVOR}" = "bookworm" ]; then \
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
