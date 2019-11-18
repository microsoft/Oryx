# Install Python build prerequisites
FROM buildpack-deps:stretch
COPY build/__pythonVersions.sh /tmp/

COPY images/build/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh && \
	apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev \
		libgeos-dev
