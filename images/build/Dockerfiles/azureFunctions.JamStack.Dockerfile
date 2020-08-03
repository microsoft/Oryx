FROM oryxdevmcr.azurecr.io/public/oryx/build:github-actions AS main

RUN oryx prep --skip-detection --platforms-and-versions nodejs=12
