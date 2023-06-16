FROM mcr.microsoft.com/dotnet/sdk:7.0

ARG AGENTBUILD=${AGENTBUILD}
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}

WORKDIR /usr/oryx
COPY build build
# This statement copies signed oryx binaries from during agent build.
# For local/dev contents of blank/empty directory named binaries are getting copied
COPY binaries /opt/buildscriptgen/
COPY src src
COPY build/FinalPublicKey.snk build/

RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish \
            -r linux-x64 \
            -o /opt/buildscriptgen/ \
            -c Release \
            src/BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \

        dotnet publish \
            -r linux-x64 \
            -o /opt/buildscriptgen/ \
            -c Release \
            src/BuildServer/BuildServer.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript
RUN chmod a+x /opt/buildscriptgen/Microsoft.Oryx.BuildServer