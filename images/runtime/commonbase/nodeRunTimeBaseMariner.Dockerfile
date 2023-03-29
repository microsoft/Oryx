ARG DEBIAN_FLAVOR
FROM oryxdevmcr.azurecr.io/private/oryx/oryx-run-base-${DEBIAN_FLAVOR}
ARG IMAGES_DIR=/tmp/oryx/images
  
