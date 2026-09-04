# syntax=docker/dockerfile:1.7

ARG UBUNTU_BASE_IMAGE=mcr.microsoft.com/mirror/docker/library/ubuntu:resolute
ARG AZURE_LINUX_BASE_IMAGE=mcr.microsoft.com/azurelinux/base/core:3.0

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
        libatomic1 \
        libbz2-1.0 \
        libexpat1 \
        libffi8 \
        libgdbm6t64 \
        libgdbm-compat4t64 \
        liblzma5 \
        libncursesw6 \
        libreadline8t64 \
        libsqlite3-0 \
        libssl3t64 \
        libuuid1 \
        tk8.6 \
        zlib1g \
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

FROM embrbase AS pythonbuildbase

RUN apt-get -o Acquire::Retries=5 update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        build-essential \
        gnupg \
        libbluetooth-dev \
        libbz2-dev \
        libffi-dev \
        libgdbm-dev \
        liblzma-dev \
        libncurses5-dev \
        libreadline-dev \
        libsqlite3-dev \
        libssl-dev \
        pkg-config \
        tk-dev \
        uuid-dev \
        wget \
        xz-utils \
        zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

FROM pythonbuildbase AS pythonbuilder

ARG PYTHON_FULL_VERSION
ARG PYTHON_GPG_KEY
ARG PYTHON_SHA256

COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN test -n "${PYTHON_FULL_VERSION}" \
    && test -n "${PYTHON_GPG_KEY}${PYTHON_SHA256}" \
    && chmod +x /tmp/receiveGpgKeys.sh \
    && mkdir -p /usr/src/python \
    && wget \
        "https://www.python.org/ftp/python/${PYTHON_FULL_VERSION%%[a-z]*}/Python-${PYTHON_FULL_VERSION}.tar.xz" \
        -O /tmp/python.tar.xz \
    && if [ -n "${PYTHON_SHA256}" ]; then \
        echo "${PYTHON_SHA256}  /tmp/python.tar.xz" | sha256sum -c -; \
    else \
        wget \
            "https://www.python.org/ftp/python/${PYTHON_FULL_VERSION%%[a-z]*}/Python-${PYTHON_FULL_VERSION}.tar.xz.asc" \
            -O /tmp/python.tar.xz.asc; \
        /tmp/receiveGpgKeys.sh "${PYTHON_GPG_KEY}"; \
        gpg --batch --verify /tmp/python.tar.xz.asc /tmp/python.tar.xz; \
    fi \
    && tar -xJf /tmp/python.tar.xz --strip-components=1 -C /usr/src/python \
    && cd /usr/src/python \
    && configure_args="" \
    && case "${PYTHON_FULL_VERSION}" in \
        3.15.*) configure_args="--without-system-libmpdec" ;; \
    esac \
    && ./configure \
        --prefix="/opt/python/${PYTHON_FULL_VERSION}" \
        --build="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)" \
        --enable-loadable-sqlite-extensions \
        --enable-optimizations \
        --enable-shared \
        --with-lto \
        --with-system-ffi \
        --without-ensurepip \
        ${configure_args} \
    && make -j "$(nproc)" \
    && make install

FROM embrbase AS main

ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION=3
ARG BUILD_NUMBER=unspecified
ARG GIT_COMMIT=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    PIP_NO_CACHE_DIR=1 \
    SSL_CERT_FILE=/etc/ssl/certs/ca-certificates.crt
LABEL com.microsoft.oryx.build-number="${BUILD_NUMBER}" \
    com.microsoft.oryx.git-commit="${GIT_COMMIT}" \
    com.microsoft.oryx.release-tag-name="${RELEASE_TAG_NAME}"

RUN test -n "${PYTHON_FULL_VERSION}" \
    && test -n "${PYTHON_VERSION}"

COPY --from=pythonbuilder \
    /opt/python/${PYTHON_FULL_VERSION} \
    /opt/python/${PYTHON_FULL_VERSION}

RUN cd /opt/python \
    && ln -s "${PYTHON_FULL_VERSION}" "${PYTHON_VERSION}" \
    && ln -s "${PYTHON_VERSION}" "${PYTHON_MAJOR_VERSION}" \
    && cd "/opt/python/${PYTHON_FULL_VERSION}/bin" \
    && ln -s python3 python \
    && ln -s python3-config python-config \
    && printf '/opt/python/%s/lib\n' "${PYTHON_MAJOR_VERSION}" \
        > /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && find "/opt/python/${PYTHON_FULL_VERSION}" -depth \
        \( -type d \( -name test -o -name tests -o -name idle_test \) \
        -o -type f \( -name '*.pyc' -o -name '*.pyo' -o -name '*.a' \) \) \
        -exec rm -rf '{}' +

ENV PATH="/opt/python/${PYTHON_MAJOR_VERSION}/bin:${PATH}" \
    PYTHON_VERSION=${PYTHON_FULL_VERSION}
