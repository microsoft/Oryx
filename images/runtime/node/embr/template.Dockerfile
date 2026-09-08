# syntax=docker/dockerfile:1.7

ARG UBUNTU_BASE_IMAGE=mcr.microsoft.com/mirror/docker/library/ubuntu:resolute
ARG AZURE_LINUX_BASE_IMAGE=mcr.microsoft.com/azurelinux/base/core:3.0

FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupcmdgen

WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT} \
    BUILD_NUMBER=${BUILD_NUMBER} \
    RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
RUN chmod +x build.sh \
    && ./build.sh node /opt/startupcmdgen/startupcmdgen

FROM ${AZURE_LINUX_BASE_IMAGE} AS azurelinuxcertificates

RUN tdnf makecache \
    && tdnf install -y ca-certificates \
    && update-ca-trust extract \
    && tdnf clean all

FROM ${UBUNTU_BASE_IMAGE} AS embrbase

RUN apt-get -o Acquire::Retries=5 update \
    && apt-get upgrade -y \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        netbase \
        openssl \
        unzip \
        zstd \
    && rm -rf /var/lib/apt/lists/*

COPY --from=azurelinuxcertificates \
    /etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem \
    /tmp/azurelinux-ca-certs/tls-ca-bundle.pem
COPY images/runtime/scripts/install-azurelinux-certs.sh /tmp/install-azurelinux-certs.sh
RUN chmod +x /tmp/install-azurelinux-certs.sh \
    && /tmp/install-azurelinux-certs.sh \
        /tmp/azurelinux-ca-certs \
        /tmp/azurelinux-ca-certs/tls-ca-bundle.pem \
    && rm -f /tmp/install-azurelinux-certs.sh

FROM embrbase AS runtimearchives

ARG NODE_FULL_VERSION
ARG NODE_SHA256
ARG YARN_VERSION
ARG YARN_URL
ARG YARN_SHA256

RUN test -n "${NODE_FULL_VERSION}${NODE_SHA256}" \
    && test -n "${YARN_VERSION}${YARN_URL}${YARN_SHA256}" \
    && apt-get -o Acquire::Retries=5 update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends xz-utils \
    && rm -rf /var/lib/apt/lists/* \
    && curl --fail --location --retry 5 --retry-all-errors \
        "https://nodejs.org/dist/v${NODE_FULL_VERSION}/node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" \
        --output /tmp/node.tar.xz \
    && echo "${NODE_SHA256}  /tmp/node.tar.xz" | sha256sum -c - \
    && mkdir -p "/opt/nodejs/${NODE_FULL_VERSION}" \
    && tar -xJf /tmp/node.tar.xz \
        --strip-components=1 \
        -C "/opt/nodejs/${NODE_FULL_VERSION}" \
    && rm -rf "/opt/nodejs/${NODE_FULL_VERSION}/include" \
        "/opt/nodejs/${NODE_FULL_VERSION}/lib/node_modules/corepack" \
        "/opt/nodejs/${NODE_FULL_VERSION}/bin/corepack" \
    && curl --fail --location --retry 5 --retry-all-errors \
        "${YARN_URL}" \
        --output /tmp/yarn.tar.gz \
    && echo "${YARN_SHA256}  /tmp/yarn.tar.gz" | sha256sum -c - \
    && mkdir -p "/opt/yarn/${YARN_VERSION}" \
    && tar -xzf /tmp/yarn.tar.gz \
        --strip-components=1 \
        -C "/opt/yarn/${YARN_VERSION}" \
    && rm -f /tmp/node.tar.xz /tmp/yarn.tar.gz

FROM embrbase AS main

ARG NODE_FULL_VERSION
ARG NODE_VERSION
ARG NODE_MAJOR_VERSION
ARG YARN_VERSION
ARG BUILD_NUMBER=unspecified
ARG GIT_COMMIT=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    NODE_VERSION=${NODE_FULL_VERSION} \
    YARN_VERSION=${YARN_VERSION} \
    SSL_CERT_FILE=/etc/ssl/certs/ca-certificates.crt
LABEL com.microsoft.oryx.build-number="${BUILD_NUMBER}" \
    com.microsoft.oryx.git-commit="${GIT_COMMIT}" \
    com.microsoft.oryx.release-tag-name="${RELEASE_TAG_NAME}"

RUN test -n "${NODE_FULL_VERSION}" \
    && test -n "${NODE_VERSION}" \
    && test -n "${NODE_MAJOR_VERSION}" \
    && test -n "${YARN_VERSION}"

COPY --from=runtimearchives \
    /opt/nodejs/${NODE_FULL_VERSION} \
    /opt/nodejs/${NODE_FULL_VERSION}
COPY --from=runtimearchives \
    /opt/yarn/${YARN_VERSION} \
    /opt/yarn/${YARN_VERSION}
COPY --from=startupcmdgen \
    /opt/startupcmdgen/startupcmdgen \
    /opt/startupcmdgen/startupcmdgen
COPY images/build/benv.sh /opt/oryx/benv

RUN cd /opt/nodejs \
    && ln -s "${NODE_FULL_VERSION}" "${NODE_VERSION}" \
    && ln -s "${NODE_VERSION}" "${NODE_MAJOR_VERSION}" \
    && chmod +x /opt/oryx/benv \
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && ln -s "/opt/nodejs/${NODE_VERSION}/bin/node" /usr/local/bin/node \
    && ln -s "/opt/nodejs/${NODE_VERSION}/bin/npm" /usr/local/bin/npm \
    && ln -s "/opt/nodejs/${NODE_VERSION}/bin/npx" /usr/local/bin/npx \
    && ln -s "/opt/yarn/${YARN_VERSION}/bin/yarn" /usr/local/bin/yarn \
    && ln -s "/opt/yarn/${YARN_VERSION}/bin/yarn" /usr/local/bin/yarnpkg

ENV PATH="/opt/nodejs/${NODE_VERSION}/bin:${PATH}"
