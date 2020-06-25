# Folders in the image which we use to build this image itself
# These are deleted in the final stage of the build
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
# Determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD

FROM oryxdevmcr.azurecr.io/public/oryx/build:github-actions AS main
ARG BUILD_DIR
ARG IMAGES_DIR

# A temporary folder to hold all content temporarily used to build this image.
# This folder is deleted in the final stage of building this image.
RUN mkdir -p ${IMAGES_DIR}
RUN mkdir -p ${BUILD_DIR}
ADD build ${BUILD_DIR}
ADD images ${IMAGES_DIR}
# chmod all script files
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# Install Node.js, NPM
FROM main AS node-install
ARG BUILD_DIR
ARG IMAGES_DIR
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN cd ${IMAGES_DIR} \
    && . ${BUILD_DIR}/__nodeVersions.sh \
    && ./installPlatform.sh nodejs $NODE12_VERSION
RUN ${IMAGES_DIR}/build/installNpm.sh
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $NODE12_VERSION /opt/nodejs/12 \
 && ln -s 12 /opt/nodejs/lts

FROM main AS final
ARG BUILD_DIR
ARG IMAGES_DIR

WORKDIR /

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Append to Oryx paths that we got from base image
ENV ORYX_PATHS="$ORYX_PATHS:/opt/nodejs/lts/bin"
# ORIGINAL_PATH represents the original value of $PATH that came from the base buildpack-deps image
ENV PATH="${ORYX_PATHS}:$ORIGINAL_PATH"

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}

RUN rm -rf /tmp/oryx