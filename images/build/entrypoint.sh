#!/bin/bash

if [ -n "$NODE" -a -d "/opt/nodejs/$NODE" ]; then
  export PATH=/opt/nodejs/$NODE/bin:$PATH
  export NPM=/opt/nodejs/$NODE/bin/npm
  export NPX=/opt/nodejs/$NODE/bin/npx
  export NODE=/opt/nodejs/$NODE/bin/node
fi
if [ -n "$PYTHON" -a -d "/opt/python/$PYTHON" ]; then
  export PATH=/opt/python/$PYTHON/bin:$PATH
  export PIP=/opt/python/$PYTHON/bin/pip
  if [ -e "/opt/python/$PYTHON/bin/virtualenv" ]; then
      export VIRTUALENV=/opt/python/$PYTHON/bin/virtualenv
  fi
  export PYTHON=/opt/python/$PYTHON/bin/python
fi

exec "$@"