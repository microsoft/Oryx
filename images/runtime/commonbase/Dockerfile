ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/debian:${DEBIAN_FLAVOR}
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build

RUN apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y --no-install-recommends \
		xz-utils \
		ca-certificates \
		curl \
		gnupg \
		netbase \
		wget

ARG DEBIAN_FLAVOR
RUN if [ "${DEBIAN_FLAVOR}" = "bookworm" ] ; then \
    apt-get update && apt-get install -y --no-install-recommends sq ; \
    fi

RUN  rm -rf /var/lib/apt/lists/*

ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
