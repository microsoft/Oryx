# Install Ruby build prerequisites
ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS ruby-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY buildV2/__rubyVersions.sh /tmp/
COPY platformsV2/ruby/prereqs/build.sh /tmp/
RUN chmod +x /tmp/build.sh