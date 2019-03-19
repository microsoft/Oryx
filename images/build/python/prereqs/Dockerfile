# Install Python build prerequisites
FROM buildpack-deps:stable
COPY ../../../../build/__python-versions.sh /tmp/
COPY build.sh /tmp/
RUN chmod +x /tmp/build.sh && \
	apt-get update && \
	apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev
