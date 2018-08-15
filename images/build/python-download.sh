#!/bin/bash
set -ex

wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz -O /python.tar.xz
wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz.asc -O /python.tar.xz.asc
gpg --keyserver ha.pool.sks-keyservers.net --recv-keys $GPG_KEY
gpg --batch --verify /python.tar.xz.asc /python.tar.xz
tar -xJf /python.tar.xz --strip-components=1 -C .
