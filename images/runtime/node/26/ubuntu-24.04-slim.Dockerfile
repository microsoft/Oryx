ARG BASE_IMAGE
ARG NODE_FULL_VERSION
ARG NODE_SHA256

# Stage 1 — build the Oryx startup-script generator (the `oryx` CLI).
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26.4-bookworm AS startupCmdGen
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh node /opt/startupcmdgen/startupcmdgen


# Stage 2 — download official Node tarball and verify SHA256.
FROM ${BASE_IMAGE} AS nodeDownloader
ARG NODE_FULL_VERSION
ARG NODE_SHA256
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates xz-utils \
    && rm -rf /var/lib/apt/lists/*
RUN set -ex \
    && cd /tmp \
    && curl -fsSLO "https://nodejs.org/dist/v${NODE_FULL_VERSION}/node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" \
    && echo "${NODE_SHA256}  node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" | sha256sum -c - \
    && mkdir -p /opt/node \
    && tar -xJf "node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" -C /opt/node --strip-components=1 \
    && rm "node-v${NODE_FULL_VERSION}-linux-x64.tar.xz" \
    && rm -rf /opt/node/share/doc /opt/node/share/man /opt/node/share/systemtap /opt/node/include


# Stage 3 — final runtime image.
FROM ${BASE_IMAGE} as main

ARG SDK_STORAGE_BASE_URL_VALUE
ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE}

# Layer 1 — runtime apt deps (least volatile; bumped only on CVE rollups).
# Targeted COPY of only the dep script — avoids invalidating this layer on
# every unrelated change under images/.
COPY images/runtime/node/26/install-dependencies-slim.sh /tmp/install-dependencies-slim.sh
RUN chmod +x /tmp/install-dependencies-slim.sh \
    && /tmp/install-dependencies-slim.sh \
    && rm -f /tmp/install-dependencies-slim.sh

# Layer 2 — Node binary tree (changes every patch bump).
ARG NODE_FULL_VERSION
ENV NODE_VERSION=${NODE_FULL_VERSION}
ENV NPM_CONFIG_LOGLEVEL=info

COPY --from=nodeDownloader /opt/node/ /usr/local/

RUN ln -sf /usr/local/bin/node /usr/local/bin/nodejs

# Layer 3 — Install yarn (1.x classic) + pnpm globally via npm. Corepack
# was removed from Node distributions in 26.x, so we install the package
# managers directly. Same `yarn` / `pnpm` binary UX on PATH.
RUN npm install --global --no-fund --no-audit yarn@1.22.22 pnpm@9.15.4

# AI + cert envs (kept for parity with other runtime images; bundled SDK is
# not installed — see Dockerfile header for rationale).
ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

# Buildpacks contract.
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

# C.UTF-8 is provided by libc — no `locales` package install required.
ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

# Layer 4 — oryx CLI (per-build, tiny — last for max cache hit on rebuilds).
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx

CMD [ "node" ]
