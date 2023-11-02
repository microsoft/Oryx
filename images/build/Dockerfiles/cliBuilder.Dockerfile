ARG DEBIAN_FLAVOR

# Use the curl flavor of buildpack-deps as the base image, which is lighter than the standard flavor; more information here: https://hub.docker.com/_/buildpack-deps
FROM buildpack-deps:${DEBIAN_FLAVOR}-curl as main
ARG DEBIAN_FLAVOR
ARG SDK_STORAGE_BASE_URL_VALUE="https://oryx-cdn.microsoft.io"
ARG AI_CONNECTION_STRING
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

# stretch was removed from security.debian.org and deb.debian.org, so update the sources to point to the archived mirror
RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.kernel.org\/debian-archive\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.kernel.org\/debian-archive\/debian stretch/g' /etc/apt/sources.list ; \
    fi

COPY --from=oryxdevmcr.azurecr.io/private/oryx/buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /opt/tmp

ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE} \
    ENABLE_DYNAMIC_INSTALL="true" \
    PATH="/usr/local/go/bin:/opt/python/latest/bin:/opt/oryx:/opt/yarn/stable/bin:/opt/hugo/lts:$PATH" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    ORYX_AI_CONNECTION_STRING="${AI_CONNECTION_STRING}" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"

# Install an assortment of traditional tooling (unicode, SSL, HTTP, etc.)
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu63 \
            libcurl4 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* ; \
    elif [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \ 
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu67 \
            libcurl4 \
            libssl1.1 \
            libyaml-dev \
            libxml2 \
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

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # .NET Core dependencies for running Oryx
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
        libunwind8 \
        rsync \
        libgdiplus \
        # Required for mysqlclient
        default-libmysqlclient-dev \
        # PHP pre-reqs
        ca-certificates \
        libargon2-0 \
        libcurl4-openssl-dev \
        libedit-dev \
        libonig-dev \
        libncurses6 \
        libsodium-dev \
        libsqlite3-dev \
        libxml2-dev \
        xz-utils \
        # ruby pre-req
        libyaml-dev \
    && rm -rf /var/lib/apt/lists/* \
    && chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "cli-builder" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

# Install Hugo and Yarn for node applications
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

# Install Python tooling for some .NET (e.g., Blazor) and node applications
RUN set -ex \
    # Upgrade system python
    && PYTHONIOENCODING="UTF-8" \
    && apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        git \
        make \
        unzip \
        build-essential \
        libpq-dev \
        moreutils \
        python3-pip \
        rsync \
        swig \
        tk-dev \
        unixodbc-dev \
        uuid-dev \
    && rm -rf /var/lib/apt/lists/*

# Install Python 3.8 to use in some .NET and node applications
RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    && buildDir="$tmpDir/build" \
    && cp -f $tmpDir/images/build/benv.sh /opt/oryx/benv \
    && cp -f $tmpDir/images/build/logger.sh /opt/oryx/logger \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger \
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
    && ln -s $PYTHON38_VERSION stable
    
ENTRYPOINT [ "benv" ]