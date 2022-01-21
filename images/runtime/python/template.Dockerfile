ARG DEBIAN_FLAVOR
# Startup script generator
FROM golang:1.14-${DEBIAN_FLAVOR} as startupCmdGen
# Install dep
RUN go get -u github.com/golang/dep/cmd/dep
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

FROM oryx-run-base-${DEBIAN_FLAVOR} AS main
ARG IMAGES_DIR=/tmp/oryx/images
ENV PYTHON_VERSION %PYTHON_FULL_VERSION%

RUN ${IMAGES_DIR}/installPlatform.sh python $PYTHON_VERSION --dir /opt/python/$PYTHON_VERSION --links false
RUN set -ex \
 && cd /opt/python/ \
 && ln -s %PYTHON_FULL_VERSION% %PYTHON_VERSION% \
 && ln -s %PYTHON_VERSION% %PYTHON_MAJOR_VERSION% \
 && echo /opt/python/%PYTHON_MAJOR_VERSION%/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "%PYTHON_MAJOR_VERSION%" = "3" ]; then cd /opt/python/%PYTHON_MAJOR_VERSION%/bin \
 && ln -s idle3 idle \
 && ln -s pydoc3 pydoc \
 && ln -s python3-config python-config; fi

ENV PATH="/opt/python/%PYTHON_MAJOR_VERSION%/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
RUN ${IMAGES_DIR}/runtime/python/install-dependencies.sh
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx
    
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
