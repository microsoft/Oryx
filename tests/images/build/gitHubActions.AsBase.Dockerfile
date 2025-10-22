ARG PARENT_IMAGE_BASE

FROM oryxdevmcr.azurecr.io/public/oryx/build:${PARENT_IMAGE_BASE} as oryx-githubactions

FROM scratch
COPY --from=oryx-githubactions / /

ENV ORYX_PATHS=/opt/oryx:/opt/nodejs/lts/bin:/opt/dotnet/sdks/lts:/opt/python/latest/bin:/opt/php/lts/bin:/opt/php-composer:/opt/yarn/stable/bin:/opt/hugo/lts

ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ENABLE_DYNAMIC_INSTALL=true \
    HOME=/home \
    LANG=C.UTF-8 \
    NUGET_PACKAGES=/var/nuget \
    NUGET_XMLDOC_MODE=skip \
    ORIGINAL_PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin \
    ORYX_SDK_STORAGE_BASE_URL=https://oryxsdksstaging.blob.core.windows.net \
    PATH=$ORYX_PATHS:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/root/.dotnet/tools:/opt/nodejs/9/bin \
    PYTHONIOENCODING=UTF-8

ENTRYPOINT [ "benv" ]