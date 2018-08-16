FROM buildpack-deps:stable AS main

# Configure locale (required for Python)
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        jq \
 && rm -rf /var/lib/apt/lists/*

# Install Node.js, NPM, Yarn
RUN curl -sL https://git.io/n-install | bash -s -- -ny - \
 && ~/n/bin/n -d 4.4.7 \
 && ~/n/bin/n -d 4.5.0 \
 && ~/n/bin/n -d 6.2.2 \
 && ~/n/bin/n -d 6.6.0 \
 && ~/n/bin/n -d 6.9.3 \
 && ~/n/bin/n -d 6.10.3 \
 && ~/n/bin/n -d 6.11.0 \
 && ~/n/bin/n -d 8.0.0 \
 && ~/n/bin/n -d 8.1.0 \
 && ~/n/bin/n -d 8.2.1 \
 && ~/n/bin/n -d 8.8.1 \
 && ~/n/bin/n -d 8.9.4 \
 && ~/n/bin/n -d 8.11.3 \
 && ~/n/bin/n -d 9.4.0 \
 && ~/n/bin/n -d 10.1.0 \
 && mv /usr/local/n/versions/node /opt/nodejs \
 && rm -rf /usr/local/n ~/n
RUN set -ex \
 && for ver in `ls /opt/nodejs`; do \
        npm_ver=`jq -r .version /opt/nodejs/$ver/lib/node_modules/npm/package.json`; \
        if [ ! -d /opt/npm/$npm_ver ]; then \
            mkdir -p /opt/npm/$npm_ver; \
            ln -s /opt/nodejs/$ver/lib/node_modules /opt/npm/$npm_ver/node_modules; \
            ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npm /opt/npm/$npm_ver/npm; \
            if [ -e /opt/nodejs/$ver/lib/node_modules/npm/bin/npx ]; then \
                chmod +x /opt/nodejs/$ver/lib/node_modules/npm/bin/npx; \
                ln -s /opt/nodejs/$ver/lib/node_modules/npm/bin/npx /opt/npm/$npm_ver/npx; \
            fi; \
        fi; \
    done
RUN set -ex \
 && YARN_VERSION=1.9.4 \
 && GPG_KEY=6A010C5166006599AA17F08146C2130DFD2497F5 \
 && gpg --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys "$GPG_KEY" || \
    gpg --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys "$GPG_KEY" || \
    gpg --keyserver hkp://pgp.mit.edu:80 --recv-keys "$GPG_KEY" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz
RUN set -ex \
 && ln -s 4.5.0 /opt/nodejs/4.5 \
 && ln -s 4.5 /opt/nodejs/4 \
 && ln -s 6.11.0 /opt/nodejs/6.11 \
 && ln -s 6.11 /opt/nodejs/6 \
 && ln -s 8.11.3 /opt/nodejs/lts \
 && ln -s 8.11.3 /opt/nodejs/8.11 \
 && ln -s 8.11 /opt/nodejs/8 \
 && ln -s 9.4.0 /opt/nodejs/9.4 \
 && ln -s 9.4 /opt/nodejs/9 \
 && ln -s 10.1.0 /opt/nodejs/latest \
 && ln -s 10.1.0 /opt/nodejs/10.1 \
 && ln -s 10.1 /opt/nodejs/10
RUN set -ex \
 && ln -s 5.6.0 /opt/npm/latest
RUN set -ex \
 && ln -s 1.9.4 /opt/yarn/stable \
 && ln -s 1.9.4 /opt/yarn/latest \
 && ln -s 1.9.4 /opt/yarn/1.9 \
 && ln -s 1.9 /opt/yarn/1
RUN set -ex \
 && cd /usr/local/bin \
 && cp -s /opt/nodejs/lts/bin/* . \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg .

# Install Python prerequisites
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8

# Build Python 2.7.15
FROM buildpack-deps:stable AS python2.7.15-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=2.7.15
ENV GPG_KEY C01E1CAD5EA2C4F0B8E3571504C367C218ADD4FF
ENV PIP_VERSION=18.0
COPY python-build.sh /
RUN /python-build.sh

# Build Python 3.5.6
FROM buildpack-deps:stable AS python3.5.6-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.5.6
ENV GPG_KEY 97FC712E4C024BBEA48A61ED3A5CA953F73C700D
ENV PIP_VERSION=18.0
COPY python-build.sh /
RUN /python-build.sh

# Build Python 3.6.6
FROM buildpack-deps:stable AS python3.6.6-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.6.6
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.0
COPY python-build.sh /
RUN /python-build.sh

# Build Python 3.7.0
FROM buildpack-deps:stable AS python3.7.0-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.7.0
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.0
COPY python-build.sh /
RUN /python-build.sh

# Install Python
FROM main AS python
COPY --from=python2.7.15-build /opt /opt
COPY --from=python3.5.6-build /opt /opt
COPY --from=python3.6.6-build /opt /opt
COPY --from=python3.7.0-build /opt /opt
RUN set -ex \
 && echo /opt/python/2.7.15/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.5.6/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.6.6/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.7.0/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN set -ex \
 && ln -s 2.7.15 /opt/python/2.7 \
 && ln -s 2.7 /opt/python/2 \
 && ln -s 3.5.6 /opt/python/3.5 \
 && ln -s 3.6.6 /opt/python/3.6 \
 && ln -s 3.7.0 /opt/python/latest \
 && ln -s 3.7.0 /opt/python/3.7 \
 && ln -s 3.7 /opt/python/3
RUN set -ex \
 && cd /usr/local/bin \
 && cp -sn /opt/python/2/bin/* . \
 && cp -sn /opt/python/3/bin/* .

FROM python AS final
COPY benv /usr/local/bin
RUN chmod +x /usr/local/bin/benv
ENTRYPOINT [ "benv" ]
CMD [ "bash" ]