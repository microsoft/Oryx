#!/bin/bash
set -ex

declare -r CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$CURRENT_DIR/__versions.sh"

baseImage="oryxdevmcr.azurecr.io/private/oryx/php-fpm-run-base"
baseImageDebianFlavor="$1"

PHP_VERSION_ARRAY=("${VERSION_ARRAY[@]}")

if [ "$baseImageDebianFlavor" == "bookworm" ];then
    PHP_VERSION_ARRAY=("${VERSION_ARRAY_BOOKWORM[@]}")
elif [ "$baseImageDebianFlavor" == "bullseye" ];then
    PHP_VERSION_ARRAY=("${VERSION_ARRAY_BULLSEYE[@]}")
elif [ "$baseImageDebianFlavor" == "buster" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BUSTER[@]}")
fi

echo
echo "Building '$baseImageDebianFlavor' based image '$baseImage' ..."
docker build \
    -t $baseImage-$baseImageDebianFlavor \
    --build-arg DEBIAN_FLAVOR=$baseImageDebianFlavor \
    -f "$CURRENT_DIR/runbase.Dockerfile" \
    .

echo "*****************"
echo "PHP_VERSION_ARRAY"
echo "${PHP_VERSION_ARRAY[@]}"
echo "*****************"

for PHP_VERSION in "${PHP_VERSION_ARRAY[@]}"
do
    IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
    VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

    PHP_IMAGE_NAME="oryxdevmcr.azurecr.io/private/oryx/php-fpm-$VERSION_DIRECTORY-$baseImageDebianFlavor"
    cd "$CURRENT_DIR/$VERSION_DIRECTORY/"

    echo
    echo "Building '$baseImageDebianFlavor' based php-fpm image '$PHP_IMAGE_NAME'..."
    echo
    docker build \
        -t $PHP_IMAGE_NAME \
        --build-arg DEBIAN_FLAVOR=$baseImageDebianFlavor \
        -f "$VERSION_DIRECTORY.$baseImageDebianFlavor.Dockerfile" \
        .
done