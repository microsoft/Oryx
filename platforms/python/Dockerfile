# Install Python build prerequisites
ARG OS_FLAVOR
# OS_FLAVOR of resolute is not in the mirror, we can still use other existing Ubuntu flavor for sdk downloading purpose
ARG BASE_IMAGE_OS=${OS_FLAVOR}
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${BASE_IMAGE_OS}
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=$VERSIONS_TO_BUILD_OVERRIDE

# COPY build/__pythonVersions.sh /tmp/

COPY platforms/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/receiveGpgKeys.sh

RUN chmod +x /tmp/build.sh && \
	apt-get update && \
	OS_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
		build-essential \ 
		tk-dev \
		uuid-dev \
		libgeos-dev

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN mkdir /tmp/sdk
RUN /tmp/platforms/python/buildPython.sh ${OS_FLAVOR} ""
