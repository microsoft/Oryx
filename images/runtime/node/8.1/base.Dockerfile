# The official Node 8.1 image has vulnerabilities, so we build our own version
# to fetch the latest stretch release with the required fixes.
# https://github.com/nodejs/docker-node.git, commit ID 2d4c7b0fdc553a5321c714526eefd3cd0cb9e074.
FROM oryx-node-run-base

RUN groupadd --gid 1000 node \
  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

ENV NPM_CONFIG_LOGLEVEL info
ENV NODE_VERSION 8.1.4

RUN curl -SLO "https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-linux-x64.tar.xz" \
  && curl -SLO --compressed "https://nodejs.org/dist/v$NODE_VERSION/SHASUMS256.txt.asc" \
  && gpg --batch --decrypt --output SHASUMS256.txt SHASUMS256.txt.asc \
  && grep " node-v$NODE_VERSION-linux-x64.tar.xz\$" SHASUMS256.txt | sha256sum -c - \
  && tar -xJf "node-v$NODE_VERSION-linux-x64.tar.xz" -C /usr/local --strip-components=1 \
  && rm "node-v$NODE_VERSION-linux-x64.tar.xz" SHASUMS256.txt.asc SHASUMS256.txt \
  && ln -s /usr/local/bin/node /usr/local/bin/nodejs

# This is a way to avoid using caches for the next stages, since we want the remaining steps
# to always run
ARG CACHEBUST=0

# Update npm to 5.10.0 to avoid this error: https://github.com/npm/npm/issues/16766
RUN curl -L https://npmjs.org/install.sh | npm_install=5.10.0 sh

RUN /tmp/scripts/installDependencies.sh
RUN rm -rf /tmp/scripts

CMD [ "node" ]


