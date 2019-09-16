FROM buildpack-deps:stretch
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN curl -sL https://git.io/n-install | bash -s -- -ny -
RUN mkdir -p /tmp/scripts
COPY platforms/node/build.sh /tmp/scripts
RUN chmod +x /tmp/scripts/build.sh
