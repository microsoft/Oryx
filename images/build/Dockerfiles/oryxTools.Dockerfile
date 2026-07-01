# Oryx Tools - Volume Mount Image
# Packages Oryx build system binaries into a minimal filesystem-only image.
# The image is mounted at /opt/oryx inside Kudu containers at runtime.

ARG BASE_IMAGE

FROM ${BASE_IMAGE} AS source

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

# Signed oryx binaries produced by the pipeline build step
COPY binaries /opt/oryx/

# Helper scripts renamed into place
COPY images/build/benv.sh   /opt/oryx/benv
COPY images/build/logger.sh /opt/oryx/logger

# Rename main binary, set permissions, write image markers
RUN mv /opt/oryx/GenerateBuildScript /opt/oryx/oryx \
    && chmod a+x /opt/oryx/oryx \
    && chmod a+x /opt/oryx/Microsoft.Oryx.BuildServer \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger \
    && echo "volume-mount" > /opt/oryx/.imagetype

FROM scratch

COPY --from=source /opt/oryx/ /
