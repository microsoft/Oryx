ARG BASE_BUILDER_TAG="20230208.1"
FROM mcr.microsoft.com/oryx/builder:${BASE_BUILDER_TAG}

# these environment variables are generally going to be overwritten:
# APP_IMAGE: name that the builder will push the final image as, e.g. testacr.azurecr.io/app/capps-runnable-app
# ACR_RESOURCE_NAME: resource name of the ACR instance, e.g. testacr.azurecr.io
# TENANT_ID: tenant id in which the ACR resides
# ACR_SCOPE: scope that the image will need to push to the ACR, e.g. repository:app/capps-runnable-app:pull,push
# MI_PRINCIPAL_ID: principal id of the managed identity that will allow the image to push to the specified ACR
ENV APP_IMAGE="" \
	ACR_RESOURCE_NAME="" \
	TENANT_ID="" \
	ACR_SCOPE="" \
	MI_PRINCIPAL_ID=""

# these environment variables are not usually overwritten
ENV	COMPRESSED_APP_LOCATION="/app-source/app.tar.gz" \
	CNB_APP_DIR="/workspace/" \
	CNB_PLATFORM_API="0.9" \
	ORYX_SDK_STORAGE_BASE_URL="https://oryx-cdn.microsoft.io" \
	ENABLE_DYNAMIC_INSTALL="true" \
	ORYX_AI_INSTRUMENTATION_KEY="4aadba6b-30c8-42db-9b93-024d5c62b887" \
	MANAGEMENT_RESOURCE_URI="https://management.core.windows.net/"

USER root

# download and install file package
RUN set -eux; \
	apt-get update; \
	apt-get install -y --no-install-recommends \
		file \
	; \
	rm -rf /var/lib/apt/lists/*

ADD startup-script.sh /startup-script.sh

RUN chmod +x /startup-script.sh

USER ${CNB_USER_ID}:${CNB_GROUP_ID}

ENTRYPOINT [ "/startup-script.sh" ]
