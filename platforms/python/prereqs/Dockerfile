# Install Python build prerequisites
ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR}
COPY build/__pythonVersions.sh /tmp/

COPY platforms/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh && \
	apt-get update && \
	apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev \
		libgeos-dev
