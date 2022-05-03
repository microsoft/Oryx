ARG DEBIAN_FLAVOR
# Startup script generator
FROM golang:1.18-${DEBIAN_FLAVOR} as startupCmdGen
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
RUN ./build.sh ruby /opt/startupcmdgen/startupcmdgen

FROM oryx-run-base-${DEBIAN_FLAVOR} AS main
ARG IMAGES_DIR=/tmp/oryx/images
ENV RUBY_VERSION %RUBY_FULL_VERSION%

RUN ${IMAGES_DIR}/installPlatform.sh ruby $RUBY_VERSION --dir /opt/ruby/$RUBY_VERSION --links false
RUN set -ex \
 && cd /opt/ruby/ \
 && ln -s %RUBY_FULL_VERSION% %RUBY_VERSION% \
 && ln -s %RUBY_VERSION% %RUBY_MAJOR_VERSION%

ENV PATH="/opt/ruby/%RUBY_MAJOR_VERSION%/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
RUN ${IMAGES_DIR}/runtime/ruby/install-dependencies.sh
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx
    
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
