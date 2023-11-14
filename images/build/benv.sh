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
# What the following lines do? They rearrange the arguments that are passed to the script so that environment variables
# come before arguments passed to benv.
# Example: 
# Let us say there is an environment variable called NODE=10.14.3
# and benv was called like 'source benv node=12.16.3', then following lines make it like 
# it was called 'source benv NODE=10.14.3 node=12.16.3'. So here the last argument's value wins.
while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^dynamic_install_root_dir=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^php=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^composer=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^python=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^node=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^dotnet=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^hugo=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^golang=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^java=')

while read benvEnvironmentVariable; do
  set -- "$benvEnvironmentVariable" "$@"
done < <(set | grep -i '^maven=')

unset benvEnvironmentVariable # Remove all traces of this part of the script

# Oryx's paths come to the end of the PATH environment variable so that any user installed platform
# sdk versions can be picked up. Here we are trying to find the first occurrence of a path like '/opt/oryx'
# and inserting a more specific provided path after it.
# Example: (note that all Oryx related patlform paths come after the typical debian paths)
# /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/opt/oryx:/opt/nodejs/6/bin:/opt/dotnet/sdks/2.2.401
updatePath() {
  if [ "$ORYX_PREFER_USER_INSTALLED_SDKS" == "true" ]
  then
    local replacingText=":/opt/oryx:$1:"
    local lookUpText=":\/opt\/oryx:"
    local currentPath="$PATH"
    local newPath=$(echo $currentPath | sed "0,/$lookUpText/ s##$replacingText#")
    export PATH="$newPath"
  else
    export PATH="$1:$PATH"
  fi
}

benv-getDynamicInstallRootDir() {
  # Iterate through arguments of the format "name=value"
  # and resolve each one, or exit if there is a failure.
  local explicitDynamicInstallRootDir=""
  while [[ $1 = *"="* ]]; do
    local name=$(echo $1 | sed 's/=.*$//')
    local value=$(echo $1 | sed 's/^.*=//')

    if matchesName "dynamic_install_root_dir" "$name"
    then
      explicitDynamicInstallRootDir="$value"
    fi
    shift
  done

  if [ -z "$explicitDynamicInstallRootDir" ]
  then
    echo "/tmp/oryx/platforms"
  else
    echo "$explicitDynamicInstallRootDir"
  fi
}

# We look at a folder structure like '/opt/nodejs/10.14.1'
benv-showSupportedVersionsErrorInfo() {
  local userPlatformName="$1"
  local platformDirName="$2"
  local userSuppliedVersion="$3"
  local dynamicInstallRootDir="$4"
  local builtInInstallDir="/opt/$platformDirName"
  local dynamicInstallDir="$dynamicInstallRootDir/$platformDirName"

  echo >&2 benv: "$userPlatformName" version \'$userSuppliedVersion\' not found.
  if [ ! -d "$builtInInstallDir" ] && [ ! -d "$dynamicInstallDir" ]; then
    echo >&2 benv: Could not find any versions on disk.
  else
    echo >&2 benv: List of available versions:
  fi

  if [ -d "$builtInInstallDir" ]; then
    benv-versions >&2 "$builtInInstallDir"
  fi

  if [ -d "$dynamicInstallDir" ]; then
    benv-versions >&2 "$dynamicInstallDir"
  fi
}

benv-getPlatformDir() {
  local platformDirName="$1"
  local userSuppliedVersion="$2"
  local dynamicInstallRootDir="$3"
  local builtInInstallDir="/opt/$platformDirName"
  local dynamicInstallDir="$dynamicInstallRootDir/$platformDirName"
  
  if [ -d "$builtInInstallDir/$userSuppliedVersion" ]; then
    echo "$builtInInstallDir/$userSuppliedVersion"
  elif [ -d "$dynamicInstallDir/$userSuppliedVersion" ]; then
    echo "$dynamicInstallDir/$userSuppliedVersion"
  else
    echo "NotFound"
  fi
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

# Since benv is going to be 'sourced', create a name which suggests it to not
# be used outside the context of benv script itself
_benvDynamicInstallRootDir=$(benv-getDynamicInstallRootDir "$@")

benv-resolve() {
  local name=$(echo $1 | sed 's/=.*$//')
  local value=$(echo $1 | sed 's/^.*=//')

  # Resolve node versions
  if matchesName "nodejs" "$name" \
    || matchesName "node" "$name" \
    || matchesName "node_version" "$name" \
    && [ "${value::1}" != "/" ]; then
    
    platformDir=$(benv-getPlatformDir "nodejs" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "node" "nodejs" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local DIR="$platformDir/bin"
    updatePath "$DIR"
    export node="$DIR/node"
    export npm="$DIR/npm"
    if [ -e "$DIR/npx" ]; then
      export npx="$DIR/npx"
    fi

    return 0
  fi

  # Resolve python versions
  if matchesName "python" "$name" || matchesName "python_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "python" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "python" "python" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    IFS='.' read -ra SPLIT_VERSION <<< "$value"
    majorAndMinorParts="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    # https://stackoverflow.com/a/4250666/1184056
    # LIBRARY_PATH is used by gcc before compilation to search directories containing static and shared libraries
    # that need to be linked to your program.
    # LD_LIBRARY_PATH is used by your program to search directories containing shared libraries after it has been
    # successfully compiled and linked.
    export LIBRARY_PATH="$platformDir/lib:$LIBRARY_PATH"
    export LD_LIBRARY_PATH="$platformDir/lib:$LD_LIBRARY_PATH"

    # C_INCLUDE_PATH tells gcc where to find C header files
    # CPLUS_INCLUDE_PATH tells gcc where to find C++ header files
    # This is a workaround solution for Python.h error which arises 
    # when python can't find specific header files:
    # https://stackoverflow.com/questions/24391964/how-can-i-get-python-h-into-my-python-virtualenv-on-mac-osx/47956013#47956013
    # Since Oryx dynamically installs SDKs in /tmp
    export C_INCLUDE_PATH="$platformDir/include/python$majorAndMinorParts"
    export CPLUS_INCLUDE_PATH="$platformDir/include/python$majorAndMinorParts"

    local binDir="$platformDir/bin"
    updatePath "$binDir"
    export python="$binDir/python$majorAndMinorParts"
    export pip="$binDir/pip"
    if [ -e "$binDir/virtualenv" ]; then
      export virtualenv="$binDir/virtualenv"
    fi

    return 0
  fi

  # Resolve hugo versions
  if matchesName "hugo" "$name" || matchesName "hugo_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "hugo" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "hugo" "hugo" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local DIR="$platformDir"
    updatePath "$DIR"
    export hugo="$DIR/hugo"

    return 0
  fi

  # Resolve PHP versions
  if matchesName "php" "$name" || matchesName "php_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "php" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "php" "php" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    # https://stackoverflow.com/a/4250666/1184056
    # LIBRARY_PATH is used by gcc before compilation to search directories containing static and shared libraries
    # that need to be linked to your program.
    # LD_LIBRARY_PATH is used by your program to search directories containing shared libraries after it has been
    # successfully compiled and linked.
    export LIBRARY_PATH="$platformDir/lib:$LIBRARY_PATH"
    export LD_LIBRARY_PATH="$platformDir/lib:$LD_LIBRARY_PATH"
    
    local DIR="$platformDir/bin"
    updatePath "$DIR"
    export php="$DIR/php"

    return 0
  fi

  # Resolve PHP versions
  if matchesName "composer" "$name" || matchesName "composer_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "php-composer" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "composer" "php-composer" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi
    export composer="$platformDir/composer.phar"
    return 0
  fi

  # Resolve dotnet versions
  if matchesName "dotnet" "$name" || matchesName "dotnet_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "dotnet" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "dotnet" "dotnet" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local SDK_DIR="$platformDir"
    toolsDir="$SDK_DIR/tools"
    if [ -d "$toolsDir" ]; then
      updatePath "$SDK_DIR/tools"
    fi

    updatePath "$SDK_DIR"
    export dotnet="$SDK_DIR/dotnet"
    
    return 0
  fi

  # Resolve Golang versions
  if matchesName "golang" "$name" || matchesName "golang_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "golang" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "golang" "golang" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local DIR="$platformDir/go/bin"
    updatePath "$DIR"
    export golang="$DIR/golang/$value"

    return 0
  fi

# Resolve java versions
  if matchesName "java" "$name" || matchesName "java_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "java" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "java" "java" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local DIR="$platformDir/bin"
    updatePath "$DIR"
    export JAVA_HOME="$platformDir"
    export java="$DIR/java"

    return 0
  fi

  # Resolve maven versions
  if matchesName "maven" "$name" || matchesName "maven_version" "$name" && [ "${value::1}" != "/" ]; then
    platformDir=$(benv-getPlatformDir "maven" "$value" "$_benvDynamicInstallRootDir")
    if [ "$platformDir" == "NotFound" ]; then
      benv-showSupportedVersionsErrorInfo "maven" "maven" "$value" "$_benvDynamicInstallRootDir"
      return 1
    fi

    local DIR="$platformDir/bin"
    updatePath "$DIR"

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