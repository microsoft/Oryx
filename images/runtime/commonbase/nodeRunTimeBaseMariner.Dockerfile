ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/private/oryx/oryx-run-base-${DEBIAN_FLAVOR}
ARG IMAGES_DIR=/tmp/oryx/images

RUN tdnf update \
  && tdnf install -y \
    sudo \
    dnf 
  
# NOTE: This is a list of keys for ALL Node versions and also Yarn package installs.
# Receiving them once speeds up building of individual node versions as they all derive
# from this image.

# Gpg keys listed at https://github.com/nodejs/node
RUN ${IMAGES_DIR}/receiveGpgKeys.sh \
    6A010C5166006599AA17F08146C2130DFD2497F5

ENV YARN_VERSION 1.22.19

RUN curl --silent --location https://dl.yarnpkg.com/rpm/yarn.repo | sudo tee /etc/yum.repos.d/yarn.repo \
  && sudo rpm --import https://dl.yarnpkg.com/rpm/pubkey.gpg \
  && sudo dnf install -y yarn 
 # && mkdir -p /opt \
 # && mv /usr/bin/yarn /opt/ \
 # && ln -s /opt/yarn-v$YARN_VERSION/bin/yarn /usr/local/bin/yarn \
 # && ln -s /opt/yarn-v$YARN_VERSION/bin/yarnpkg /usr/local/bin/yarnpkg 

  
