# The official Node 6.9 image has vulnerabilities, so we build our own version
# to fetch the latest stretch release with the required fixes.
# https://github.com/nodejs/docker-node.git, commit ID a0c6d581ef9724999c5c8e2a21a387e741d677cc
FROM oryx-node-run-base-stretch

RUN groupadd --gid 1000 node \
  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

ENV NPM_CONFIG_LOGLEVEL info
ENV NODE_VERSION 6.9.5

ARG IMAGES_DIR=/tmp/oryx/images
RUN ${IMAGES_DIR}/installPlatform.sh nodejs $NODE_VERSION --dir /usr/local --links false \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs
RUN ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

CMD [ "node" ]

