ARG DEBIAN_FLAVOR
# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.18-${DEBIAN_FLAVOR} as startupCmdGen
# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM oryxdevmcr.azurecr.io/private/oryx/%BASE_TAG% as main
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        xz-utils \
    && rm -rf /var/lib/apt/lists/*

ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

ENV PYTHON_VERSION %PYTHON_FULL_VERSION%
RUN true
COPY build/__pythonVersions.sh ${BUILD_DIR}
RUN true
COPY platforms/__common.sh /tmp/
RUN true
COPY platforms/python/prereqs/build.sh /tmp/
RUN true
COPY platforms/python/versionsToBuild.txt /tmp/
RUN true
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN true

RUN chmod +x /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh

RUN ${BUILD_DIR}/buildPythonSdkByVersion.sh $PYTHON_VERSION $DEBIAN_FLAVOR

RUN set -ex \
 && cd /opt/python/ \
 && ln -s %PYTHON_FULL_VERSION% %PYTHON_VERSION% \
 && ln -s %PYTHON_VERSION% %PYTHON_MAJOR_VERSION% \
 && echo /opt/python/%PYTHON_MAJOR_VERSION%/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "%PYTHON_MAJOR_VERSION%" = "3" ]; then cd /opt/python/%PYTHON_MAJOR_VERSION%/bin \
 && ln -nsf idle3 idle \
 && ln -nsf pydoc3 pydoc \
 && ln -nsf python3-config python-config; fi \
 && rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/python/%PYTHON_MAJOR_VERSION%/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

RUN ${IMAGES_DIR}/runtime/python/install-dependencies.sh
RUN pip install --upgrade pip \
    && pip install gunicorn \
    && pip install debugpy \
    && if [ "%PYTHON_MAJOR_VERSION%" = "3" ] && [ "%PYTHON_VERSION%" != "3.6" ]; then pip install viztracer==0.14.3 \
    && pip install vizplugins==0.1.2 \
    && pip install orjson==3.6.6; fi \
    && if [ "%PYTHON_VERSION%" = "3.7" ] || [ "%PYTHON_VERSION%" = "3.8" ]; then curl -LO http://ftp.de.debian.org/debian/pool/main/libf/libffi/libffi6_3.2.1-9_amd64.deb \
    && dpkg -i libffi6_3.2.1-9_amd64.deb \
    && rm libffi6_3.2.1-9_amd64.deb; fi \
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
