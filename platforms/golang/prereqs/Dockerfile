# Install Golang build prerequisites
ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS golang-build-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY build/__golangVersions.sh /tmp/
COPY platforms/golang/prereqs/build.sh /tmp/
RUN chmod +x /tmp/build.sh