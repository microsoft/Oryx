ARG DEBIAN_FLAVOR
# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.20-${DEBIAN_FLAVOR} as startupCmdGen
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
RUN ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM oryxdevmcr.azurecr.io/private/oryx/oryx-run-base-bullseye as main
ARG DEBIAN_FLAVOR
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
ARG SDK_STORAGE_BASE_URL_VALUE
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}
ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE}

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        xz-utils \
    && rm -rf /var/lib/apt/lists/*

ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

ENV PYTHON_VERSION 3.10.8
RUN true
COPY build/__pythonVersions.sh ${BUILD_DIR}
RUN true
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
 && ln -s 3.10.8 3.10 \
 && ln -s 3.10 3 \
 && echo /opt/python/3/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "3" = "3" ]; then cd /opt/python/3/bin \
 && ln -nsf idle3 idle \
 && ln -nsf pydoc3 pydoc \
 && ln -nsf python3-config python-config; fi \
 && rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/python/3/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

RUN ${IMAGES_DIR}/runtime/python/install-dependencies.sh
RUN pip install --upgrade pip \
    && pip install gunicorn \
    && pip install uvicorn \
    && pip install debugpy \
    && pip install viztracer==0.15.6 \
    && pip install vizplugins==0.1.3 \
    && pip install orjson==3.8.1 \
    && if [ "3.10" = "3.7" ] || [ "3.10" = "3.8" ]; then curl -LO http://ftp.de.debian.org/debian/pool/main/libf/libffi/libffi6_3.2.1-9_amd64.deb \
    && dpkg -i libffi6_3.2.1-9_amd64.deb \
    && rm libffi6_3.2.1-9_amd64.deb; fi \
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx

ENV LANG="en_US.UTF-8" \
    LANGUAGE="en_US.UTF-8" \
    LC_ALL="en_US.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
