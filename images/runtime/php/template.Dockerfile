ARG DEBIAN_FLAVOR
# Startup script generator
FROM golang:1.18-${DEBIAN_FLAVOR} as startupCmdGen

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM %RUNTIME_BASE_IMAGE_NAME%

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && rm -rf /tmp/oryx \
    # Temporarily making sure apache2-foreground has permission
    && chmod +x /usr/local/bin/apache2-foreground
