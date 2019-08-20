#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

benv-versions() {
  local IFS=$' \r\n'
  local version
  for version in $(ls "$1"); do
    local link=$(readlink "$1/$version" || echo -n)
    if [ -z "$link" ]; then
      echo "  $version"
    else
      echo "  $version -> $link"
    fi
  done
}

benv-resolve() {
  local name=$(echo $1 | sed 's/=.*$//')
  local value=$(echo $1 | sed 's/^.*=//')

  # Resolve node versions
  if [ "$name" == "node" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/nodejs/$value" ]; then
      echo >&2 benv: node version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/nodejs
      return 1
    fi

    local DIR="/opt/nodejs/$value/bin"
    export PATH="$DIR:$PATH"
    export node="$DIR/node"
    export npm="$DIR/npm"
    if [ -e "$DIR/npx" ]; then
      export npx="$DIR/npx"
    fi

    return 0
  fi

  # Resolve npm versions
  if [ "$name" == "npm" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/npm/$value" ]; then
      echo >&2 benv: npm version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/npm
      return 1
    fi

    local DIR="/opt/npm/$value"
    export PATH="$DIR:$PATH"
    export npm="$DIR/npm"
    if [ -e "$DIR/npx" ]; then
      export npx="$DIR/npx"
    fi

    return 0
  fi

  # Resolve python versions
  if [ "$name" == "python" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/python/$value" ]; then
      echo >&2 benv: python version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/python
      return 1
    fi

    local DIR="/opt/python/$value/bin"
    export PATH="$DIR:$PATH"
    if [ -e "$DIR/python2" ]; then
      export python="$DIR/python2"
    elif [ -e "$DIR/python3" ]; then
      export python="$DIR/python3"
    fi
    export pip="$DIR/pip"
    if [ -e "$DIR/virtualenv" ]; then
      export virtualenv="$DIR/virtualenv"
    fi

    return 0
  fi

  # Resolve PHP versions
  if [ "$name" == "php" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/php/$value" ]; then
      echo >&2 benv: php version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/php
      return 1
    fi

    local DIR="/opt/php/$value/bin"
    export PATH="$DIR:$PATH"
    export php="$DIR/php"

    return 0
  fi

  # Resolve dotnet versions
  if [ "$name" == "dotnet" ] && [ "${value::1}" != "/" ]; then
    local runtimesDir="/opt/dotnet/runtimes"
    if [ ! -d "$runtimesDir/$value" ]; then
      echo >&2 benv: dotnet version \'$value\' not found\; choose one of:
      benv-versions >&2 $runtimesDir
      return 1
    fi

    local DIR=$(readlink $"$runtimesDir/$value/sdk")
    export PATH="$DIR:$PATH"
    export dotnet="$DIR/dotnet"
    
    return 0
  fi

  # Export other names without resolution
  eval export $name\=\'${value//\'/\'\\\'\'}\'
}

# Iterate through arguments of the format "name=value"
# and resolve each one, or exit if there is a failure.
while [[ $1 = *"="* ]]; do
  benv-resolve "$1" || if [ "$0" != "$BASH_SOURCE" ]; then
    # Remove all traces of this script prior to returning
    unset -f benv-resolve benv-versions;
    return 1;
  else
    exit 1;
  fi
  shift
done

if [ "$0" != "$BASH_SOURCE" ]; then
  # Remove all traces of this script prior to returning
  unset -f benv-resolve benv-versions
  if [ $# -gt 0 ]; then
    source "$@"
  fi
else
  if [ $# -eq 0 ]; then
    if [ $$ -eq 1 ]; then
      set -- bash
    else
      set -- env
    fi
  fi
  exec "$@"
fi