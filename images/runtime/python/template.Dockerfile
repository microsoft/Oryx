ARG DEBIAN_FLAVOR
ARG BASE_IMAGE

# Startup script generator
# Using 1.20 golang image because golang latest image is not supported for buster, so using 1.20 golang image and then updating it.
# TODO: Once buster gets deprecated, update the golang base image
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.20-${DEBIAN_FLAVOR} as startupCmdGen

# Download and install the latest version of Go
RUN curl -OL https://go.dev/dl/go1.25.7.linux-amd64.tar.gz && \
    rm -rf /usr/local/go && \
    tar -C /usr/local -xzf go1.25.7.linux-amd64.tar.gz && \
    rm go1.25.7.linux-amd64.tar.gz
ENV PATH=$PATH:/usr/local/go/bin
# Verify the installation
RUN go version
# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM ${BASE_IMAGE} as main

ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
ARG SDK_STORAGE_BASE_URL_VALUE

ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}
ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE}

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        xz-utils \
        # Install gcc due to error installing viztracer returning gcc not found
        gcc \
    && rm -rf /var/lib/apt/lists/*

ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

ARG PYTHON_FULL_VERSION
ARG PYTHON_VERSION
ARG PYTHON_MAJOR_VERSION

ENV PYTHON_VERSION ${PYTHON_FULL_VERSION}
RUN true
# COPY build/__pythonVersions.sh ${BUILD_DIR}
# RUN true
COPY platforms/__common.sh /tmp/
RUN true
COPY platforms/python/prereqs/build.sh /tmp/
RUN true
COPY platforms/python/versions/${DEBIAN_FLAVOR}/versionsToBuild.txt /tmp/
RUN true
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN true

RUN chmod +x /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh

RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    && ${BUILD_DIR}/buildPythonSdkByVersion.sh $PYTHON_VERSION $DEBIAN_FLAVOR

RUN set -ex \
 && cd /opt/python/ \
 && ln -s ${PYTHON_FULL_VERSION} ${PYTHON_VERSION} \
 && ln -s ${PYTHON_VERSION} ${PYTHON_MAJOR_VERSION} \
 && echo /opt/python/${PYTHON_MAJOR_VERSION}/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "${PYTHON_MAJOR_VERSION}" = "3" ]; then cd /opt/python/${PYTHON_MAJOR_VERSION}/bin \
 && ln -nsf idle3 idle \
 && ln -nsf pydoc3 pydoc \
 && ln -nsf python3-config python-config; fi \
 && rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/python/${PYTHON_MAJOR_VERSION}/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

RUN ${IMAGES_DIR}/runtime/python/install-dependencies.sh
RUN --mount=type=secret,id=pip_index_url,target=/run/secrets/pip_index_url \
    pip install --index-url $(cat /run/secrets/pip_index_url) --upgrade pip && \
    pip install --index-url $(cat /run/secrets/pip_index_url) gunicorn debugpy viztracer==0.15.6 vizplugins==0.1.3 && \
    if [ "${PYTHON_VERSION}" != "3.12" ] && [ "${PYTHON_VERSION}" != "3.7" ]; then pip install --index-url $(cat /run/secrets/pip_index_url) orjson==3.10.7; fi && \
    if [ "${PYTHON_VERSION}" = "3.7" ] || [ "${PYTHON_VERSION}" = "3.8" ]; then curl -LO http://ftp.de.debian.org/debian/pool/main/libf/libffi/libffi6_3.2.1-9_amd64.deb \
    && dpkg -i libffi6_3.2.1-9_amd64.deb \
    && rm libffi6_3.2.1-9_amd64.deb; fi && \
    ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/oryx

ENV LANG="en_US.UTF-8" \
    LANGUAGE="en_US.UTF-8" \
    LC_ALL="en_US.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
