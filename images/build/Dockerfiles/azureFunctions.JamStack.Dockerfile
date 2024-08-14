ARG BASE_IMAGE
ARG DEBIAN_FLAVOR
FROM ${BASE_IMAGE} AS main

ARG IMAGES_DIR=/tmp/images
ARG BUILD_DIR=/tmp/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

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
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
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

ARG YARN_VERSION
ARG YARN_MINOR_VERSION
ARG YARN_MAJOR_VERSION

COPY images/yarn-v$YARN_VERSION.tar.gz .
RUN set -e \
    && yarnCacheFolder="/usr/local/share/yarn-cache" \
    && mkdir -p $yarnCacheFolder \
    && chmod 777 $yarnCacheFolder \
    && mkdir -p /opt/yarn \
    && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
    && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
    && rm yarn-v$YARN_VERSION.tar.gz

COPY nodejs-${DEBIAN_FLAVOR}-16.20.0.tar.gz .
RUN set -e \
    && mkdir -p /opt/nodejs/16.20.0 \
    && tar -xzf nodejs-${DEBIAN_FLAVOR}-16.20.0.tar.gz -C /usr/local \
    && rm nodejs-${DEBIAN_FLAVOR}-16.20.0.tar.gz \
    && ln -sfn "/opt/nodejs/16.20.0" "/opt/nodejs/16.20"

RUN set -ex \
    && ln -s $YARN_VERSION /opt/yarn/stable \
    && ln -s $YARN_VERSION /opt/yarn/latest \
    && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
    && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
    && mkdir -p /links \
    && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links
                       
ARG PYTHON38_VERSION
ENV PYTHON38_VERSION ${PYTHON38_VERSION}
COPY python-${DEBIAN_FLAVOR}-${PYTHON38_VERSION}.tar.gz .

RUN set -e \
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
    && mkdir -p /opt/python/${PYTHON38_VERSION} \
    && tar -xzf python-${DEBIAN_FLAVOR}-${PYTHON38_VERSION}.tar.gz -C /opt/python/${PYTHON38_VERSION} \
    && rm python-${DEBIAN_FLAVOR}-${PYTHON38_VERSION}.tar.gz \
    && [ -d "/opt/python/$PYTHON38_VERSION" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -s $PYTHON38_VERSION 3.8 \
    && ln -s $PYTHON38_VERSION latest \
    && ln -s $PYTHON38_VERSION stable \
    && echo "jamstack" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype