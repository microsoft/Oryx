FROM python:3.10-buster
SHELL ["/bin/bash", "-c"]
ENV PYTHON_VERSION 3.10.0

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        build-essential \
        zlib1g-dev \
        libncurses5-dev \
        libgdbm-dev \
        libnss3-dev \
        libssl-dev \
        libreadline-dev \
        libffi-dev \
        libsqlite3-dev \
        wget \
        libbz2-dev \
    && rm -rf /var/lib/apt/lists/* \
RUN cd /opt/python/ \
    && wget https://www.python.org/ftp/python/3.10.0/Python-3.10.0.tgz \
    && tar -xf Python-3.10.0.tgz \
    && mv Python-3.10.0 3.10 \
    && cd 3.10 \
    && ./configure --enable-optimizations \
    && make -j 4 \
    && make altinstall

ARG IMAGES_DIR=/tmp/oryx/images
RUN ${IMAGES_DIR}/installPlatform.sh python $PYTHON_VERSION --dir /opt/python/$PYTHON_VERSION --links false \
    && ln -s /usr/local/bin/python /usr/local/bin/python
RUN ${IMAGES_DIR}/runtime/python/installDependencies.sh
RUN rm -rf /tmp/oryx