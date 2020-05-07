#!/bin/bash
set -ex

declare -r CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$CURRENT_DIR/__versions.sh"

baseImage="php-fpm-run-base-buster"
echo
echo "Buildig image '$baseImage'..."
docker build -t $baseImage -f "$CURRENT_DIR/runbase.Buster.Dockerfile" .

for PHP_VERSION in "${VERSION_ARRAY_BUSTER[@]}"
do
    IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
    VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    PHP_IMAGE_NAME="php-fpm-$VERSION_DIRECTORY-buster"
    cd "$CURRENT_DIR/$VERSION_DIRECTORY/"

    echo
    echo "Building php image '$PHP_IMAGE_NAME'..."
    echo
    docker build -t $PHP_IMAGE_NAME -f "$VERSION_DIRECTORY.buster.Dockerfile" .
done