ARG OS_FLAVOR
ARG BASE_IMAGE

# Startup script generator
FROM mcr.microsoft.com/mirror/docker/library/ubuntu:noble AS startupCmdGen

ARG GO_VERSION=1.25.1
ARG GO_SHA256="7716a0d940a0f6ae8e1f3b3f4f36299dc53e31b16840dbd171254312c41ca12e"
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

RUN apt-get update \
 && apt-get install -y --no-install-recommends curl ca-certificates git build-essential \
 && curl -fsSL https://go.dev/dl/go${GO_VERSION}.linux-amd64.tar.gz -o go.tgz \
 && echo "${GO_SHA256}  go.tgz" | sha256sum -c - \
 && tar -C /usr/local -xzf go.tgz \
 && rm go.tgz

ENV GOPATH=/go \
    PATH=/go/bin:/usr/local/go/bin:$PATH \
    GIT_COMMIT=${GIT_COMMIT} \
    BUILD_NUMBER=${BUILD_NUMBER} \
    RELEASE_TAG_NAME=${RELEASE_TAG_NAME} \
    PATH_CA_CERTIFICATE=/etc/ssl/certs/ca-certificate.crt

RUN mkdir -p "$GOPATH/src" "$GOPATH/bin" && chmod -R 1777 "$GOPATH"
    
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

RUN chmod +x build.sh && ./build.sh python /opt/startupcmdgen/startupcmdgen
RUN apt-get purge -y build-essential git curl && apt-get autoremove -y && rm -rf /var/lib/apt/lists/*


FROM ${BASE_IMAGE} as main

ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build

ARG OS_FLAVOR
ENV OS_FLAVOR=${OS_FLAVOR}

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
COPY platforms/__common.sh /tmp/
COPY platforms/python/prereqs/build.sh /tmp/
# COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

# RUN chmod +x /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh

RUN ${BUILD_DIR}/buildPythonSdkByVersion.sh $PYTHON_VERSION $OS_FLAVOR

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
RUN pip install --upgrade pip \
    && pip install gunicorn \
    && pip install debugpy \
    && pip install viztracer==0.15.6 \
    && pip install vizplugins==0.1.3 \
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx

ENV LANG="en_US.UTF-8" \
    LANGUAGE="en_US.UTF-8" \
    LC_ALL="en_US.UTF-8"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
