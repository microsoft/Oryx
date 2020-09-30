FROM oryxdevmcr.azurecr.io/public/oryx/build:github-actions AS main
ARG ORYX_BUILDIMAGE_TYPE
ENV ORYX_BUILDIMAGE_TYPE="${ORYX_BUILDIMAGE_TYPE}"

RUN oryx prep --skip-detection --platforms-and-versions nodejs=12
