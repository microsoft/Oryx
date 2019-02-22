#!/bin/bash

# Translate environment variables into script arguments by
# reading well known names from the current environment and
# prepending each one found to the current script's arguments.
# Since arguments are prepended, the order in which they are
# added is backwards so that the final ordering is correct.
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep '^python_')
[ -n "$python" ] && set -- "python=$python" "$@"

while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep '^php_')
[ -n "$php" ] && set -- "php=$php" "$@"

while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep '^npm_')
[ -n "$npm" ] && set -- "npm=$npm" "$@"

while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep '^node_')
[ -n "$node" ] && set -- "node=$node" "$@"

while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep '^dotnet_')
[ -n "$dotnet" ] && set -- "dotnet=$dotnet" "$@"

unset benvvar # Remove all traces of this part of the script

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
  if [ "$name" == "node" -o "${name::5}" == "node_" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/nodejs/$value" ]; then
      echo >&2 benv: node version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/nodejs
      return 1
    fi
    local DIR="/opt/nodejs/$value/bin"
    if [ "$name" == "node" ]; then
      export PATH="$DIR:$PATH"
      export node="$DIR/node"
      export npm="$DIR/npm"
      if [ -e "$DIR/npx" ]; then
        export npx="$DIR/npx"
      fi
    else
      eval export node_${name:5}=\"$DIR/node\"
      eval export npm_${name:5}=\"$DIR/npm\"
      if [ -e "$DIR/npx" ]; then
        eval export npx_${name:5}=\"$DIR/npx\"
      fi
    fi
    return 0
  fi

  # Resolve npm versions
  if [ "$name" == "npm" -o "${name::4}" == "npm_" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/npm/$value" ]; then
      echo >&2 benv: npm version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/npm
      return 1
    fi
    local DIR="/opt/npm/$value"
    if [ "$name" == "npm" ]; then
      export PATH="$DIR:$PATH"
      export npm="$DIR/npm"
      if [ -e "$DIR/npx" ]; then
        export npx="$DIR/npx"
      fi
    else
      eval export npm_${name:4}=\"$DIR/npm\"
      if [ -e "$DIR/npx" ]; then
        eval export npx_${name:4}=\"$DIR/npx\"
      fi
    fi
    return 0
  fi

  # Resolve python versions
  if [ "$name" == "python" -o "${name::7}" == "python_" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/python/$value" ]; then
      echo >&2 benv: python version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/python
      return 1
    fi
    local DIR="/opt/python/$value/bin"
    if [ "$name" == "python" ]; then
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
    else
      if [ -e "$DIR/python2" ]; then
        eval export python_${name:7}=$DIR/python2
      elif [ -e "$DIR/python3" ]; then
        eval export python_${name:7}=$DIR/python3
      fi
      eval export pip_${name:7}="$DIR/pip"
      if [ -e "$DIR/virtualenv" ]; then
        eval export virtualenv_${name:7}=$DIR/virtualenv
      fi
    fi
    return 0
  fi

  # Resolve PHP versions
  if [ "$name" == "php" -o "${name::7}" == "php_" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/php/$value" ]; then
      echo >&2 benv: php version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/php
      return 1
    fi
    local DIR="/opt/php/$value/bin"
    if [ "$name" == "php" ]; then
      export PATH="$DIR:$PATH"
      export php="$DIR/php"
    else
      eval export php_${name:7}=\"$DIR/php\"
    fi
    return 0
  fi

  # Resolve dotnet versions
  if [ "$name" == "dotnet" -o "${name::11}" == "dotnet_" ] && [ "${value::1}" != "/" ]; then
    if [ ! -d "/opt/dotnet/$value" ]; then
      echo >&2 benv: dotnet version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/dotnet
      return 1
    fi
    local DIR="/opt/dotnet/$value"
    if [ "$name" == "dotnet" ]; then
      export PATH="$DIR:$PATH"
      export dotnet="$DIR/dotnet"
    else
      eval export dotnet_${name:7}=\"$DIR/dotnet\"
    fi
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