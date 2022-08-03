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
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM mcr.microsoft.com/oryx/base:%RUNTIME_BASE_IMAGE_TAG%

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && rm -rf /tmp/oryx \
    # Temporarily making sure apache2-foreground has permission
    && chmod +x /usr/local/bin/apache2-foreground
