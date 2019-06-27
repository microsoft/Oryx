# Startup script generator
FROM golang:1.11-stretch as startupCmdGen
# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen

FROM %DOTNETCORE_BASE_IMAGE%

# Older .NET core versions, which have reached end of life and therefore are no longer updated, use
# a version of `curl` that has known issues.
# We manually update it here so we can still depend on the original images.
# This command should be removed once support for deprecated .NET core images is halted.
RUN sed -i '/jessie-updates/d' /etc/apt/sources.list  # Now archived

RUN apt-get update \
  && apt-get install -y \
     curl \
     file \
  && rm -rf /var/lib/apt/lists/*

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx