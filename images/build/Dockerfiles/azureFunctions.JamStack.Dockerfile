ARG PARENT_DEBIAN_FLAVOR
ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/cli:${PARENT_DEBIAN_FLAVOR} AS main

COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /tmp

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PATH="/usr/local/go/bin:/opt/python/latest/bin:$PATH" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    ORYX_PATHS="/opt/oryx:/opt/nodejs/lts/bin:/opt/python/latest/bin:/opt/yarn/stable/bin"

# stretch was removed from security.debian.org and deb.debian.org, so update the sources to point to the archived mirror
RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.kernel.org\/debian-archive\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.kernel.org\/debian-archive\/debian stretch/g' /etc/apt/sources.list ; \
    fi

ENV DEBIAN_FRONTEND=noninteractive

RUN set -ex \
    # Install Python SDKs
    # Upgrade system python
    && PYTHONIOENCODING="UTF-8" \
    # It's not clear whether these are needed at runtime...
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # Adding additional python packages to support all optional python modules:
        # https://devguide.python.org/getting-started/setup-building/index.html#install-dependencies
        apt-utils \
        git \
        make \
        unzip \
        # The tools in this package are used when installing packages for Python
        build-essential \
        moreutils \
        python3-pip \
        swig \
        tk-dev \
        unixodbc-dev \
        uuid-dev \
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
        # Required for PostgreSQL
        libpq-dev \
        # Required for mysqlclient
        default-libmysqlclient-dev \
        # Required for ts
        zip \
        #TODO : Add these to fix php failures. Check if these can be removed.
        libargon2-0 \
        libonig-dev \
        libedit-dev \
    && rm -rf /var/lib/apt/lists/* \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx

ARG IMAGES_DIR="/opt/tmp/images"
ARG BUILD_DIR="/opt/tmp/build"

RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    && yarnCacheFolder="/usr/local/share/yarn-cache" \
    && mkdir -p $yarnCacheFolder \
    && chmod 777 $yarnCacheFolder \
    && . ${BUILD_DIR}/__nodeVersions.sh \
    && if [ "${DEBIAN_FLAVOR}" == "bullseye" || "${DEBIAN_FLAVOR}" == "buster" ]; then ${IMAGES_DIR}/installPlatform.sh nodejs ${NODE16_VERSION}; fi \
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
  
RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    # Install Python SDKs
    # Upgrade system python
    && PYTHONIOENCODING="UTF-8" \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    && mkdir -p /usr/local/share/pip-cache/lib \
    && chmod -R 777 /usr/local/share/pip-cache \
    && pip3 install pip --upgrade \
    && pip install --upgrade cython \
    && pip3 install --upgrade cython \
    && . $buildDir/__pythonVersions.sh \
    && $imagesDir/installPlatform.sh python $PYTHON38_VERSION \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -s $PYTHON38_VERSION 3.8 \
    && ln -s $PYTHON38_VERSION latest \
    && ln -s $PYTHON38_VERSION stable \
    && echo "jamstack" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype