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
 && ln -s 8.11.3 /opt/nodejs/lts \
 && ln -s /opt/nodejs/lts/bin/node /usr/local/bin/node \
 && ln -s /opt/nodejs/lts/bin/npm /usr/local/bin/npm \
 && ln -s /opt/nodejs/lts/bin/npx /usr/local/bin/npx \
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
 && ln -s /opt/yarn/$YARN_VERSION/bin/yarn /usr/local/bin/yarn \
 && ln -s /opt/yarn/$YARN_VERSION/bin/yarnpkg /usr/local/bin/yarnpkg \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

# Install Python prerequisites
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tk-dev \
        uuid-dev \
 && rm -rf /var/lib/apt/lists/*
# https://github.com/docker-library/python/issues/147
ENV PYTHONIOENCODING UTF-8

# Build Python 2.7
FROM buildpack-deps:stable AS python2.7-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=2.7.15
ENV GPG_KEY C01E1CAD5EA2C4F0B8E3571504C367C218ADD4FF
ENV PIP_VERSION=18.0
COPY python-download.sh /
RUN /python-download.sh
COPY python2-build.sh /
RUN /python2-build.sh
COPY python-install.sh /
RUN /python-install.sh
RUN ln -s $PYTHON_VERSION /opt/python/2.7

# Build Python 3.5
FROM buildpack-deps:stable AS python3.5-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.5.6
ENV GPG_KEY 97FC712E4C024BBEA48A61ED3A5CA953F73C700D
ENV PIP_VERSION=18.0
COPY python-download.sh /
RUN /python-download.sh
COPY python3-build.sh /
RUN /python3-build.sh
COPY python-install.sh /
RUN /python-install.sh
RUN ln -s $PYTHON_VERSION /opt/python/3.5

# Build Python 3.6
FROM buildpack-deps:stable AS python3.6-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.6.6
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.0
COPY python-download.sh /
RUN /python-download.sh
COPY python3-build.sh /
RUN /python3-build.sh
COPY python-install.sh /
RUN /python-install.sh
RUN ln -s $PYTHON_VERSION /opt/python/3.6

# Build Python 3.7
FROM buildpack-deps:stable AS python3.7-build
WORKDIR /usr/src/python
ENV PYTHON_VERSION=3.7.0
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
ENV PIP_VERSION=18.0
COPY python-download.sh /
RUN /python-download.sh
COPY python3-build.sh /
RUN /python3-build.sh
COPY python-install.sh /
RUN /python-install.sh
RUN ln -s $PYTHON_VERSION /opt/python/3.7

# Install Python
FROM main AS python
COPY --from=python2.7-build /opt /opt
COPY --from=python3.5-build /opt /opt
COPY --from=python3.6-build /opt /opt
COPY --from=python3.7-build /opt /opt
RUN echo /opt/python/2.7/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.5/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.6/lib >> /etc/ld.so.conf.d/python.conf \
 && echo /opt/python/3.7/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig
RUN ln -s /opt/python/2.7/bin/python2 /usr/local/bin/python2 \
 && ln -s python2 /usr/local/bin/python
RUN ln -s /opt/python/3.7/bin/python3 /usr/local/bin/python3

FROM python AS final
COPY entrypoint.sh /
ENTRYPOINT [ "/entrypoint.sh" ]
CMD [ "bash" ]