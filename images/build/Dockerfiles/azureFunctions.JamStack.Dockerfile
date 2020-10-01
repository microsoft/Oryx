FROM oryxdevmcr.azurecr.io/public/oryx/build:github-actions AS main

ENV ORYX_BUILDIMAGE_TYPE="jamstack"

RUN oryx prep --skip-detection --platforms-and-versions nodejs=12 \
    && echo "jamstack" > /opt/oryx/.imagetype