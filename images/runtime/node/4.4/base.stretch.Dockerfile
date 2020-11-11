# The official Node 4.4 image has vulnerabilities, so we build our own version
# to fetch the latest stretch release with the required fixes.
# https://github.com/nodejs/docker-node.git, commit ID 22668206915e4d39c4c35608848be835dd5526a3
FROM oryx-node-run-base-stretch
ENV NPM_CONFIG_LOGLEVEL info
ENV NODE_VERSION 4.4.7

ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
RUN . ${BUILD_DIR}/__sdkStorageConstants.sh \
    && ${IMAGES_DIR}/installPlatform.sh -p nodejs -v $NODE_VERSION -b /usr/local --use-specified-dir
RUN ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

CMD [ "node" ]
