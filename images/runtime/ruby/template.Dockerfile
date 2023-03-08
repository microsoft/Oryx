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
RUN ./build.sh ruby /opt/startupcmdgen/startupcmdgen

FROM oryxdevmcr.azurecr.io/private/oryx/oryx-run-base-${DEBIAN_FLAVOR} AS main
ARG IMAGES_DIR=/tmp/oryx/images
ARG DEBIAN_FLAVOR
ENV RUBY_VERSION %RUBY_FULL_VERSION%
ENV DEBIAN_FLAVOR=${DEBIAN_FLAVOR}

RUN ${IMAGES_DIR}/installPlatform.sh ruby $RUBY_VERSION --dir /opt/ruby/$RUBY_VERSION --links false
RUN set -ex \
 && cd /opt/ruby/ \
 && ln -s %RUBY_FULL_VERSION% %RUBY_VERSION% \
 && ln -s %RUBY_VERSION% %RUBY_MAJOR_VERSION%

ENV PATH="/opt/ruby/%RUBY_MAJOR_VERSION%/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

RUN ${IMAGES_DIR}/runtime/ruby/install-dependencies.sh
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/* \
    && rm -rf /tmp/oryx
    
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"