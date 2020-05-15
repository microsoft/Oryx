#!/bin/bash
set -ex

declare -r CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$CURRENT_DIR/__versions.sh"

baseImage="php-run-base"
baseImageType="$1"

echo
echo "Buildig '$1' based image '$baseImage'..."
docker build \
    -t $baseImage-$baseImageType \
    --build-arg RUNIMAGE_BASE=$baseImageType \
    -f "$CURRENT_DIR/runbase.Dockerfile" \
    .

PHP_VERSION_ARRAY=("${VERSION_ARRAY[@]}")

if [ "$baseImageType" == "buster" ];then
	PHP_VERSION_ARRAY=("${VERSION_ARRAY_BUSTER[@]}")
fi

for PHP_VERSION in "${PHP_VERSION_ARRAY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$PHP_VERSION"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

	PHP_IMAGE_NAME="php-$VERSION_DIRECTORY-$baseImageType"
    cd "$CURRENT_DIR/$VERSION_DIRECTORY/"

    echo
    echo "Building php image '$PHP_IMAGE_NAME'..."
    echo
	docker build \
        -t $PHP_IMAGE_NAME \
        --build-arg RUNIMAGE_BASE=$baseImageType \
        -f "$VERSION_DIRECTORY.Dockerfile" \
        .
done