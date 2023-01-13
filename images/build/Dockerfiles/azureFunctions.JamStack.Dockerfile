ARG PARENT_DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/cli-bullseye:${PARENT_DEBIAN_FLAVOR} AS main
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
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
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
        #TODO : Add these to fix php failures. Check if these can be removed.
        libargon2-0 \
        libonig-dev \
        libedit-dev \
    && rm -rf /var/lib/apt/lists/* \
    # This is the folder containing 'links' to benv and build script generator
    && mkdir -p /opt/oryx
ARG IMAGES_DIR="/opt/tmp/images"
ARG BUILD_DIR="/opt/tmp/build"
ARG HUGO_DIR="/opt/hugo"
RUN oryx prep --skip-detection --platforms-and-versions nodejs \
    && echo "jamstack" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype \
    && rm -rf ${HUGO_DIR}


