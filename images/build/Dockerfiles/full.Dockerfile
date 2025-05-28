ARG DEBIAN_FLAVOR

### oryx run-script image
# DisableDockerDetector "Below image not yet supported in the Docker Hub mirror"
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.23.1-${DEBIAN_FLAVOR} as startupScriptGens

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}
RUN chmod +x build.sh && ./build.sh golang     /opt/startupcmdgen/golang

### oryx build image
FROM buildpack-deps:${DEBIAN_FLAVOR}-curl
ARG DEBIAN_FLAVOR
ARG SDK_STORAGE_BASE_URL_VALUE
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE}

ARG IMAGES_DIR=/opt/tmp/images
ARG BUILD_DIR=/opt/tmp/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

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
    
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/

RUN if [ "${DEBIAN_FLAVOR}" = "bullseye" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu67 \
            libcurl4 \
            libssl1.1 \
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

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
        make \
        unzip \
        vim \
        # Required for ts
        moreutils \
        rsync \
        zip \
        libgdiplus \
        jq \
    # .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/* \
    && chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "full" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

RUN set -ex \
    && tmpDir="/opt/tmp" \
    && imagesDir="$tmpDir/images" \
    # enables custom logging
    && cp -f $imagesDir/build/logger.sh /opt/oryx/logger

ENV ENABLE_DYNAMIC_INSTALL="true" \
    PATH="$PATH:/opt/oryx" \