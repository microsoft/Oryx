ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
ARG AGENTBUILD
FROM mcr.microsoft.com/dotnet/core/sdk:2.1
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Common /usr/oryx/src/Common
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
# This statement copies signed oryx binaries from during agent build.
# For local/dev contents of blank/empty directory named binaries are getting copied
COPY binaries /opt/buildscriptgen/
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG AGENTBUILD=${AGENTBUILD}
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ARG AGENTBUILD=${AGENTBUILD}
RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript
