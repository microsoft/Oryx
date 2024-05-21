# Install Python build prerequisites
ARG DEBIAN_FLAVOR
# DEBIAN_HACK_FLAVOR is only needed for python 3.10 or newer
ARG DEBIAN_HACK_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_HACK_FLAVOR}
ARG DEBIAN_FLAVOR
ARG DEBIAN_HACK_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
ENV DEBIAN_HACK_FLAVOR=$DEBIAN_HACK_FLAVOR

COPY buildV2/__pythonVersions.sh /tmp/

COPY platformsV2/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/receiveGpgKeys.sh

# stretch was removed from security.debian.org and deb.debian.org, so update the sources to point to the archived mirror
RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
    fi

RUN chmod +x /tmp/build.sh && \
	apt-get update && \
	DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
		build-essential \ 
		tk-dev \
		uuid-dev \
		libgeos-dev