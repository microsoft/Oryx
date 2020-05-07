FROM debian:buster-slim
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build

RUN apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y --no-install-recommends \
		ca-certificates \
		curl \
		netbase \
		wget \
		xz-utils \
	&& rm -rf /var/lib/apt/lists/*

RUN set -ex; \
	if ! command -v gpg > /dev/null; then \
		apt-get update; \
		apt-get install -y --no-install-recommends \
			gnupg \
			dirmngr \
		; \
		rm -rf /var/lib/apt/lists/*; \
	fi
	
ADD images ${IMAGES_DIR}
ADD build ${BUILD_DIR}
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
