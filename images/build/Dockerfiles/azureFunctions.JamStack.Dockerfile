#ARG PARENT_DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/public/oryx/cli-bullseye:debian-bullseye AS main
ARG DEBIAN_FLAVOR

COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /tmp

ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR \
    ORYX_BUILDIMAGE_TYPE="jamstack" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PATH="/usr/local/go/bin:/opt/python/latest/bin:$PATH" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

ARG IMAGES_DIR="/opt/tmp/images"
RUN oryx prep --skip-detection --platforms-and-versions nodejs=14 --debug \
    && echo "$DEBIAN_FLAVOR" \
    && . /tmp/build/__goVersions.sh \
    && downloadedFileName="go${GO_VERSION}.linux-amd64.tar.gz" \
    && ${IMAGES_DIR}/retry.sh "curl -SLsO https://golang.org/dl/$downloadedFileName" \
    && mkdir -p /usr/local \
    && gzip -d $downloadedFileName \
    && tar -xf "go${GO_VERSION}.linux-amd64.tar" -C /usr/local \
    && rm -rf $downloadedFileName



