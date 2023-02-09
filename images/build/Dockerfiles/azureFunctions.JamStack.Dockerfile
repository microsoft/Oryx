ARG PARENT_DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/build:github-${PARENT_DEBIAN_FLAVOR} AS main
ARG DEBIAN_FLAVOR

COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /tmp

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PATH="/usr/local/go/bin:/opt/python/latest/bin:$PATH" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

ARG IMAGES_DIR="/opt/tmp/images"
RUN oryx prep --skip-detection --platforms-and-versions nodejs=14 --debug \
    && echo "$DEBIAN_FLAVOR" \
    && . /tmp/build/__goVersions.sh \
    && downloadedFileName="go${GO_VERSION}.linux-amd64.tar.gz" \
    && ${IMAGES_DIR}/retry.sh "curl -SLsO https://golang.org/dl/$downloadedFileName" \
    && mkdir -p /usr/local \
    && gzip -d $downloadedFileName \
    && tar -xf "go${GO_VERSION}.linux-amd64.tar" -C /usr/local \
    && rm -rf $downloadedFileName

RUN set -ex \
    # Install Python SDKs
    # Upgrade system python
    && PYTHONIOENCODING="UTF-8" \
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        # Adding additional python packages to support all optional python modules:
        # https://devguide.python.org/getting-started/setup-building/index.html#install-dependencies
        build-essential \
        python3-pip \
        swig \
        tk-dev \
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

    && rm -rf /var/lib/apt/lists/*

RUN set -ex \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
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