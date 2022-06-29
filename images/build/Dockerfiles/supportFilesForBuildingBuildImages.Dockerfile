FROM oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-stretch
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;