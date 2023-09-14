ARG BASE_BUILDER_IMAGE="cormtestacr.azurecr.io/oryx/builder:debian-bullseye-20230821.1"
FROM ${BASE_BUILDER_IMAGE}

# these environment variables are generally going to be overwritten:
# APP_IMAGE: name that the builder will push the final image as, e.g. testacr.azurecr.io/app/capps-runnable-app
# ACR_RESOURCE_NAME: resource name of the ACR instance, e.g. testacr.azurecr.io
# TENANT_ID: tenant id in which the ACR resides
# ACR_SCOPE: scope that the image will need to push to the ACR, e.g. repository:app/capps-runnable-app:pull,push
# MI_PRINCIPAL_ID: principal id of the managed identity that will allow the image to push to the specified ACR
ENV APP_IMAGE="" \
	ACR_RESOURCE_NAME="private-repository-k8s.k8se-system" \
	TENANT_ID="" \
	ACR_SCOPE="" \
	MI_PRINCIPAL_ID="" \
	REGISTRY_AUTH_USERNAME="" \
	REGISTRY_AUTH_PASSWORD=""

# these environment variables are not usually overwritten
ENV	COMPRESSED_APP_LOCATION="/app-source/app.tar.gz" \
	CNB_APP_DIR="/workspace/" \
	CNB_EXTENSIONS_DIR="/cnb/extensions" \
	CNB_PLATFORM_API="0.12" \
	CNB_ANALYZED_PATH="/analyzed/analyzed.toml" \
	CNB_EXPERIMENTAL_MODE=warn \
	ORYX_SDK_STORAGE_BASE_URL="https://oryx-cdn.microsoft.io" \
	ENABLE_DYNAMIC_INSTALL="true" \
	ORYX_AI_CONNECTION_STRING="InstrumentationKey=4aadba6b-30c8-42db-9b93-024d5c62b887" \
	MANAGEMENT_RESOURCE_URI="https://management.core.windows.net/"

USER root

# download and install file package
RUN set -eux; \
	apt-get update; \
	apt-get install -y --no-install-recommends \
		file \
		ca-certificates \
		sudo \
	; \
	rm -rf /var/lib/apt/lists/*

ADD startup-script.sh /startup-script.sh

COPY app.tar.gz /${COMPRESSED_APP_LOCATION}

RUN chmod u+x /startup-script.sh

# allow cnb user to run update-ca-certificates
RUN touch /usr/local/share/ca-certificates/internalregistry.crt \
	&& chmod 666 /usr/local/share/ca-certificates/internalregistry.crt
RUN chown ${CNB_USER_ID}:${CNB_GROUP_ID} /etc/ssl/certs/ \
	&& chown ${CNB_USER_ID}:${CNB_GROUP_ID} /usr/local/share/ca-certificates/

# Give cnb user permission to /cnb/extensions directory
RUN mkdir -p ${CNB_EXTENSIONS_DIR} && \
	chown -R ${CNB_USER_ID}:${CNB_GROUP_ID} ${CNB_EXTENSIONS_DIR}

# Give cnb user permission to /kaniko directory
RUN mkdir -p /kaniko && \
	chown -R ${CNB_USER_ID}:${CNB_GROUP_ID} /kaniko

# Give cnb user permission to /analyzed directory
RUN mkdir -p /analyzed && \
	chown -R ${CNB_USER_ID}:${CNB_GROUP_ID} /analyzed

# Give cnb user permission to /cache directory
RUN mkdir -p /cache && \
	chown -R ${CNB_USER_ID}:${CNB_GROUP_ID} /cache

USER ${CNB_USER_ID}:${CNB_GROUP_ID}

ENTRYPOINT [ "/startup-script.sh" ]
