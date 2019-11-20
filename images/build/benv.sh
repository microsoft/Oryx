#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Perform case-insensitive comparison
matchesName() {
  local expectedName="$1"
  local providedName="$2"
  local result=
  shopt -s nocasematch
  [[ "$expectedName" == "$providedName" ]] && result=0 || result=1
  shopt -u nocasematch
   return $result
}

# Read the environment variables to see if a value for these variables have been set.
# If a variable was set as an environment variable AND as an argument to benv script, then the argument wins.
# Example:
#   export dotnet=1
#   source benv dotnet=3
#   dotnet --version (This should print version 3)
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^php=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^php_version=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^python=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^python_version=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^node=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^node_version=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^npm=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^npm_version=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^dotnet=')
while read benvvar; do
  set -- "$benvvar" "$@"
done < <(set | grep -i '^dotnet_version=')
unset benvvar # Remove all traces of this part of the script

# Oryx's paths come to the end of the PATH environment variable so that any user installed platform
# sdk versions can be picked up. Here we are trying to find the first occurrence of a path like '/opt/'
# (as in /opt/dotnet) and inserting a more specific provided path before it.
# Example: (note that all Oryx related patlform paths come in the end)
# /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/opt/nodejs/6/bin:/opt/dotnet/sdks/2.2.401:/opt/oryx
updatePath() {
  local replacingText="$1:/opt/"
  local currentPath="$PATH"
  local lookUpText="\/opt\/"
  local newPath=$(echo $currentPath | sed "0,/$lookUpText/ s##$replacingText#")
  export PATH="$newPath"
}

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

benv-getVersions() {
    local rootDir="$1"
    local versions=""
    for i in $(find "$rootDir" -maxdepth 1)
    do
        local name=$(basename "$i")
        if [ ! -z --versions "$versions" ]; then
            versions+=","
        fi
        versions+="$name"
    done
    echo $versions
}

benv-resolve() {
  # Splits the string based on the first occurrence of '='
  # First occurrence is important because the value could have a '=' itself
  # For example, benv dotnet="=2.1.14"
  IFS="=" read -r name value <<< "$1"

  # Resolve node versions
  if matchesName "node" "$name" || matchesName "node_version" "$name" && [ "${value::1}" != "/" ]; then
    versions=`benv-getVersions /opt/nodejs`
    resolvedVersion=`oryx resolveVersion "$value" --versions "$versions"`

    if [ -z "$resolvedVersion" ]; then
      echo >&2 benv: node version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/nodejs
      return 1
    fi

    local DIR="/opt/nodejs/$resolvedVersion/bin"
    updatePath "$DIR"
    export node="$DIR/node"
    export npm="$DIR/npm"
    if [ -e "$DIR/npx" ]; then
      export npx="$DIR/npx"
    fi

    return 0
  fi

  # Resolve npm versions
  if matchesName "npm" "$name" || matchesName "npm_version" "$name" && [ "${value::1}" != "/" ]; then
    versions=`benv-getVersions /opt/npm`
    resolvedVersion=`oryx resolveVersion "$value" --versions "$versions"`
    
    if [ -z "$resolvedVersion" ]; then
      echo >&2 benv: npm version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/npm
      return 1
    fi

    local DIR="/opt/npm/$resolvedVersion"
    updatePath "$DIR"
    export npm="$DIR/npm"
    if [ -e "$DIR/npx" ]; then
      export npx="$DIR/npx"
    fi

    return 0
  fi

  # Resolve python versions
  if matchesName "python" "$name" || matchesName "python_version" "$name" && [ "${value::1}" != "/" ]; then
    versions=`benv-getVersions /opt/python`
    resolvedVersion=`oryx resolveVersion "$value" --versions "$versions"`

    if [ -z "$resolvedVersion" ]; then
      echo >&2 benv: python version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/python
      return 1
    fi

    local DIR="/opt/python/$resolvedVersion/bin"
    updatePath "$DIR"
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
  if matchesName "php" "$name" || matchesName "php_version" "$name" && [ "${value::1}" != "/" ]; then
    versions=`benv-getVersions /opt/php`
    resolvedVersion=`oryx resolveVersion "$value" --versions "$versions"`

    if [ -z "$resolvedVersion" ]; then
      echo >&2 benv: php version \'$value\' not found\; choose one of:
      benv-versions >&2 /opt/php
      return 1
    fi

    local DIR="/opt/php/$resolvedVersion/bin"
    updatePath "$DIR"
    export php="$DIR/php"

    return 0
  fi

  # Resolve dotnet versions
  if matchesName "dotnet" "$name" || matchesName "dotnet_version" "$name" && [ "${value::1}" != "/" ]; then
    local runtimesDir="/opt/dotnet/runtimes"
    versions=`benv-getVersions $runtimesDir`
    resolvedVersion=`oryx resolveVersion "$value" --versions "$versions"`
    
    if [ -z "$resolvedVersion" ]; then
      echo >&2 benv: dotnet version \'$value\' not found\; choose one of:
      benv-versions >&2 $runtimesDir
      return 1
    fi

    local SDK_DIR=$(readlink $"$runtimesDir/$resolvedVersion/sdk")

    toolsDir="$SDK_DIR/tools"
    if [ -d "$toolsDir" ]; then
      updatePath "$SDK_DIR/tools"
    fi

    updatePath "$SDK_DIR"
    export dotnet="$SDK_DIR/dotnet"
    
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