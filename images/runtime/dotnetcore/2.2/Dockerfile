# Startup script generator
FROM golang:1.11-stretch as startupCmdGen
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

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.7

RUN apt-get update \
  && apt-get install -y \
     file \
     lldb \
  && rm -rf /var/lib/apt/lists/*

COPY --from=tools-install /dotnetcore-tools /opt/dotnetcore-tools
ENV PATH="/opt/dotnetcore-tools:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx