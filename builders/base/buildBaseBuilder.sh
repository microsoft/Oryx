#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
printf -v DATE_TAG '%(%Y-%m-%d-%H-%M-%S)T' -1

# constants
declare -r ORYX_BUILDPACK_IMAGE_PLACEHOLDER="%ORYX_BUILDPACK_IMAGE%"
declare -r ORYX_BUILDPACK_VERSION_PLACEHOLDER="%ORYX_BUILDPACK_VERSION%"
declare -r ORYX_RUN_STACK_IMAGE_PLACEHOLDER="%ORYX_RUN_STACK_IMAGE%"
declare -r ORYX_BUILD_STACK_IMAGE_PLACEHOLDER="%ORYX_BUILD_STACK_IMAGE%"

acrName="oryxprodmcr"

PARAMS=""
while (( "$#" )); do
	case "$1" in
		-a|--acr-name)
		acrName=$2
		shift 2
		;;
		--) # end argument parsing
		shift
		break
		;;
		-*|--*=) # unsupported flags
		echo "Error: Unsupported flag $1" >&2
		exit 1
		;;
		*) # preserve positional arguments
		PARAMS="$PARAMS $1"
		shift
		;;
	esac
done

echo "Tagging all images with the tag: $DATE_TAG"
echo "-------------------------------------------------"

if [[ $rebuildStack == "true" ]]; then
    echo
    echo "Rebuilding stack..."
    echo
    baseImage="$acrName.azurecr.io/oryxplusplus/local-app-no-docker:stack-base-$DATE_TAG"
    buildStackImage="$acrName.azurecr.io/oryxplusplus/local-app-no-docker:stack-build-$DATE_TAG"
    runStackImage="$acrName.azurecr.io/oryxplusplus/local-app-no-docker:stack-run-$DATE_TAG"
    docker build ./stack/ -t $baseImage --target base
    docker build ./stack/ -t $runStackImage --target run
    docker build ./stack/ -t $buildStackImage --target build
    docker push $baseImage
    docker push $runStackImage
    docker push $buildStackImage
    echo "-------------------------------------------------"

#     echo
#     cat << EOF
# Please overwrite the [stack] section in './builder/builder.toml' to be:
# -------------------------------
# [stack]
# id = "oryx.stacks.skeleton"
# # This image is used at runtime
# run-image = "$runStackImage"
# # This image is used at build-time
# build-image = "$buildStackImage"
# -------------------------------
# EOF
#     echo
#     user_input=""
#     while [[ ! $user_input == "y" ]]; do
#         read -p "Type y to indicate that './builder/builder.toml' has been updated: " user_input
#     done
#     echo "-------------------------------------------------"
fi




buildPackImage="$acrName.azurecr.io/oryxplusplus/local-app-no-docker:buildpack-$DATE_TAG"
echo
echo "Packaging buildpack image: $buildPackImage"
echo
pack buildpack package $buildPackImage --config $SCRIPT_DIR/packaged-buildpack/package.toml
docker push $buildPackImage
echo "-------------------------------------------------"

# echo
# cat << EOF
# Please overwrite the [[buildpacks]] section in './builder/builder.toml' to be:
# -------------------------------
# [[buildpacks]]
# image = "$buildPackImage"
# -------------------------------
# EOF
# echo

# user_input=""
# while [[ ! $user_input == "y" ]]; do
#     read -p "Type y to indicate that './builder/builder.toml' has been updated: " user_input
# done
# echo "-------------------------------------------------"

# Copy template.builder.toml over to builder.toml and replace placeholders
builderTomlTemplate="$SCRIPT_DIR/builder/template.builder.toml"
targetBuilderToml="$SCRIPT_DIR/builder/builder.toml"
cp "$builderTomlTemplate" "$targetBuilderToml"
sed -i "s|$ORYX_BUILDPACK_IMAGE_PLACEHOLDER|$buildPackImage|g" "$targetBuilderToml"
sed -i "s|$ORYX_BUILDPACK_VERSION_PLACEHOLDER|$VERSION_DIRECTORY|g" "$targetBuilderToml"
sed -i "s|$ORYX_RUN_STACK_IMAGE_PLACEHOLDER|$runStackImage|g" "$targetBuilderToml"
sed -i "s|$ORYX_BUILD_STACK_IMAGE_PLACEHOLDER|$buildStackImage|g" "$targetBuilderToml"

builderImage="$acrName.azurecr.io/oryxplusplus/local-app-no-docker:$DATE_TAG"
echo
echo "Creating builder image: $builderImage"
echo
pack builder create $builderImage --config $SCRIPT_DIR/builder/builder.toml
echo "-------------------------------------------------"