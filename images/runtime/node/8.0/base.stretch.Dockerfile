# The official Node 8.0 image has vulnerabilities, so we build our own version
# to fetch the latest stretch release with the required fixes.
# https://github.com/nodejs/docker-node.git, commit ID 91c4846ffc53042451696de9e194dcfd28d1e236.
FROM oryxdevmcr.azurecr.io/private/oryx/oryx-node-run-base-stretch

RUN groupadd --gid 1000 node \
  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

ENV NPM_CONFIG_LOGLEVEL info
ENV NODE_VERSION 8.0.0

ARG IMAGES_DIR=/tmp/oryx/images
RUN ${IMAGES_DIR}/installPlatform.sh nodejs $NODE_VERSION --dir /usr/local --links false \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

# This is a way to avoid using caches for the next stages, since we want the remaining steps
# to always run
ARG CACHEBUST=0

# Update npm to 5.10.0 to avoid this error: https://github.com/npm/npm/issues/16766
RUN curl -L https://npmjs.org/install.sh | npm_install=5.10.0 sh

RUN ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

CMD [ "node" ]
