FROM oryxdevmcr.azurecr.io/public/oryx/build:github-actions AS main

RUN oryx prep --skip-detection --platforms-and-versions nodejs=12

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}
