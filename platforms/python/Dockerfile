# Install Python build prerequisites
ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR}
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR

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
