ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/private/oryx/oryx-node-run-base-${DEBIAN_FLAVOR}

RUN groupadd --gid 1000 node \
  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

RUN ARCH= && dpkgArch="$(dpkg --print-architecture)" \
  && case "${dpkgArch##*-}" in \
    amd64) ARCH='x64';; \
    ppc64el) ARCH='ppc64le';; \
    s390x) ARCH='s390x';; \
    arm64) ARCH='arm64';; \
    armhf) ARCH='armv7l';; \
    i386) ARCH='x86';; \
    *) echo "unsupported architecture"; exit 1 ;; \
  esac

ARG NODE14_VERSION
ARG DEBIAN_FLAVOR
ENV NODE_VERSION ${NODE14_VERSION}
ENV NPM_CONFIG_LOGLEVEL info

ARG BUILD_DIR=/tmp/oryx/build
ARG IMAGES_DIR=/tmp/oryx/images
RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    && ${IMAGES_DIR}/installPlatform.sh nodejs $NODE_VERSION --dir /usr/local --links false \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs
RUN . ${BUILD_DIR}/__nodeVersions.sh \
    && npm install -g npm@${NPM_VERSION}
RUN ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

CMD [ "node" ]


