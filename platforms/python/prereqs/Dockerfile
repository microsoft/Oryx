# Install Python build prerequisites
FROM buildpack-deps:stretch
COPY build/__pythonVersions.sh /tmp/

COPY platforms/python/prereqs/build.sh /tmp/
COPY images/receivePgpKeys.sh /tmp/receivePgpKeys.sh
RUN chmod +x /tmp/receivePgpKeys.sh
RUN chmod +x /tmp/build.sh && \
	apt-get update && \
	apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev \
		libgeos-dev
