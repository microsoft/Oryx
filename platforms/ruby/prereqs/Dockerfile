# Install Ruby build prerequisites
FROM buildpack-deps:stretch AS ruby-build-prereqs
COPY build/__rubyVersions.sh /tmp/
COPY platforms/ruby/prereqs/build.sh /tmp/
RUN chmod +x /tmp/build.sh