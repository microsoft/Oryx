#!/bin/bash
set -ex

make install

PYTHON_PATH=/opt/python/$PYTHON_VERSION

wget https://bootstrap.pypa.io/get-pip.py -O /get-pip.py
LD_LIBRARY_PATH=/usr/src/python \
/usr/src/python/python /get-pip.py \
    --prefix $PYTHON_PATH \
    --disable-pip-version-check \
    --no-cache-dir \
    --no-warn-script-location \
    pip==$PIP_VERSION

if [ -e "$PYTHON_PATH/bin/python3" ]; then
    LD_LIBRARY_PATH=$PYTHON_PATH/lib \
    $PYTHON_PATH/bin/pip install --no-cache-dir virtualenv
fi
