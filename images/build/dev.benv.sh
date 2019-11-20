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
  # Splits the string based on the first occurrence of '='
  # First occurrence is important because the value could have a '=' itself
  # For example, benv dotnet="=2.1.14"
  IFS="=" read -r name value <<< "$1"

  source ~/.bashrc

  # Resolve node versions
  if matchesName "node" "$name" || matchesName "node_version" "$name" && [ "${value::1}" != "/" ]; then
    resolvedVersion=`oryx resolveVersion "$value" --platform node`
    echo "Installing NodeJS version $resolvedVersion..."
    nvm install $resolvedVersion
    return 0
  fi

  # Resolve python versions
  if matchesName "python" "$name" || matchesName "python_version" "$name" && [ "${value::1}" != "/" ]; then
    resolvedVersion=`oryx resolveVersion "$value" --platform python`
    echo "Installing Python version $resolvedVersion..."
    pyenv install $resolvedVersion
    export PYENV_VERSION=$resolvedVersion
    return 0
  fi

  # Resolve PHP versions
  if matchesName "php" "$name" || matchesName "php_version" "$name" && [ "${value::1}" != "/" ]; then
    resolvedVersion=`oryx resolveVersion "$value" --platform php`
    echo "Installing PHP version $resolvedVersion..."
    phpbrew install $resolvedVersion
    return 0
  fi

  # Resolve dotnet versions
  if matchesName "dotnet" "$name" || matchesName "dotnet_version" "$name" && [ "${value::1}" != "/" ]; then
    resolvedVersion=`oryx resolveVersion "$value" --platform dotnet`
    echo "Installing .NET Core SDK version $resolvedVersion..."
    apt-get install -y --no-install-recommends dotnet-sdk-$resolvedVersion
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
