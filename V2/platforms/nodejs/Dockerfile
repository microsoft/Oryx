ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${DEBIAN_FLAVOR}
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
    fi

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        make \
        jq \
    && rm -rf /var/lib/apt/lists/*
RUN curl -sL https://git.io/n-install | bash -s -- -ny -

RUN mkdir -p /tmp/scripts
COPY platforms/nodejs/build.sh /tmp/scripts
RUN chmod +x /tmp/scripts/build.sh

COPY platforms /tmp/platforms
COPY build /tmp/build
RUN chmod +x /tmp/platforms/**/*.sh
RUN chmod +x /tmp/build/**/*.sh
RUN /tmp/platforms/nodejs/getNode.sh ${DEBIAN_FLAVOR} ""