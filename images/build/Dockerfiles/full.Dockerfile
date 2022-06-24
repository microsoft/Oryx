ARG DEBIAN_FLAVOR

### oryx run-script image
FROM golang:1.15-${DEBIAN_FLAVOR} as startupScriptGens

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
RUN ./build.sh golang     /opt/startupcmdgen/golang

### oryx build image
FROM buildpack-deps:${DEBIAN_FLAVOR}-curl
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

# docker multi-stage builds
COPY --from=buildscriptgenerator /opt/ /opt/
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/

RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
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

ENV ORYX_SDK_STORAGE_BASE_URL="https://oryx-cdn.microsoft.io"
ENV ENABLE_DYNAMIC_INSTALL="true"
ENV PATH="$PATH:/opt/oryx"