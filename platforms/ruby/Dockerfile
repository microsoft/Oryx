FROM oryxdevmcr.azurecr.io/private/oryx/ruby-build-prereqs
WORKDIR /usr/src/ruby
ARG RUBY_VERSION
ARG GEM_VERSION
ARG RUBY_SHA256
RUN RUBY_VERSION=$RUBY_VERSION \
    GEM_VERSION=$GEM_VERSION \
    RUBY_SHA256=$RUBY_SHA256 \
    /tmp/build.sh