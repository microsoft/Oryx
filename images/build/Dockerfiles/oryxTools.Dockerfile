# Oryx Tools - Volume Mount Image
# Packages Oryx build system binaries into a minimal filesystem-only image.
# LWAS volume-mounts this image at /opt/oryx inside the Kudu container at runtime.

ARG BASE_IMAGE

FROM ${BASE_IMAGE} AS source

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ARG DEBIAN_FLAVOR
ARG OS_FLAVOR

ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}

WORKDIR /usr/oryx
COPY build build
# Signed oryx binaries produced by the pipeline build step
COPY binaries /opt/buildscriptgen/
COPY src src
COPY build/FinalPublicKey.snk build/

RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && chmod a+x /opt/buildscriptgen/Microsoft.Oryx.BuildServer

ARG IMAGES_DIR=/opt/tmp/images
ARG BUILD_DIR=/opt/tmp/build
RUN mkdir -p ${IMAGES_DIR} \
    && mkdir -p ${BUILD_DIR}
COPY images ${IMAGES_DIR}
COPY build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \; \
    && find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# Assemble /opt/oryx with all artifacts required for the volume mount
RUN mkdir -p /opt/oryx \
    && cp /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && cp /opt/buildscriptgen/Microsoft.Oryx.BuildServer /opt/oryx/Microsoft.Oryx.BuildServer \
    && chmod a+x /opt/oryx/oryx \
    && chmod a+x /opt/oryx/Microsoft.Oryx.BuildServer \
    && cp -P /opt/buildscriptgen/*.dll /opt/oryx/ 2>/dev/null || true \
    && cp -P /opt/buildscriptgen/*.so /opt/oryx/ 2>/dev/null || true \
    && cp -P /opt/buildscriptgen/*.json /opt/oryx/ 2>/dev/null || true \
    && cp -f $IMAGES_DIR/build/benv.sh /opt/oryx/benv \
    && cp -f $IMAGES_DIR/build/logger.sh /opt/oryx/logger \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger \
    && echo "volume-mount" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

FROM scratch

COPY --from=source /opt/oryx/ /
