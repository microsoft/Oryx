# Install Ruby build prerequisites
ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS ruby-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY build/__rubyVersions.sh /tmp/
COPY platforms/ruby/prereqs/build.sh /tmp/
RUN chmod +x /tmp/build.sh