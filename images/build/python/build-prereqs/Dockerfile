# Install Python build prerequisites
FROM buildpack-deps:stable AS python-build-prereqs
COPY build/__python-versions.sh /tmp/
COPY images/build/python/build.sh /
RUN chmod +x /build.sh
RUN apt-get update && \
	apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev
