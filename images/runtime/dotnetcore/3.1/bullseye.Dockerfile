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
RUN ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen

FROM mcr.microsoft.com/oryx/base:dotnetcore-3.1-20220810.1

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ARG USER_DOTNET_AI_VERSION
ENV USER_DOTNET_AI_VERSION=${USER_DOTNET_AI_VERSION}
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
ENV DOTNET_VERSION=3.1
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN echo $USER_DOTNET_AI_VERSION && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get install unzip -y \ 
    && apt-get upgrade --assume-yes \
    && mkdir -p /DotNetCoreAgent \
    && curl -o /DotNetCoreAgent/appinsights.zip "https://oryxsdksdev.blob.core.windows.net/appinsights-agent/DotNetCoreAgent.$USER_DOTNET_AI_VERSION.zip" \
    && cd DotNetCoreAgent \
    && unzip appinsights.zip && rm appinsights.zip