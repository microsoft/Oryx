ARG BASE_IMAGE
FROM ${BASE_IMAGE}
ARG OS_FLAVOR=resolute
ENV OS_FLAVOR=${OS_FLAVOR}
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=${VERSIONS_TO_BUILD_OVERRIDE}
ARG YARN_VERSION=1.22.22
ENV YARN_VERSION=${YARN_VERSION}

RUN . /etc/os-release; \
    if [ "${OS_FLAVOR}" != "resolute" ] || [ "${ID}" != "ubuntu" ] || [ "${VERSION_CODENAME}" != "resolute" ]; then \
        echo "Embr Node SDK builds require an Ubuntu Resolute base and OS_FLAVOR=resolute." >&2; \
        exit 1; \
    fi

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl jq xz-utils gnupg dirmngr \
    && rm -rf /var/lib/apt/lists/*

COPY platforms/nodejs/build.sh /tmp/scripts/build.sh
RUN chmod +x /tmp/scripts/build.sh
COPY platforms /tmp/platforms
COPY build /tmp/build
COPY images/constants.yml /tmp/images/constants.yml
RUN mkdir -p /tmp/sdk

# Prepare only the disposable EMBR container's manifest; App Service is unchanged.
RUN set -eu; \
    manifest="$(mktemp)"; \
    remaining="${VERSIONS_TO_BUILD_OVERRIDE},"; \
    while [ -n "${remaining}" ]; do \
        version="$(printf '%s' "${remaining%%,*}" | xargs)"; \
        remaining="${remaining#*,}"; \
        if ! printf '%s\n' "${version}" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+$'; then \
            echo "Invalid Embr Node SDK version '${version}'." >&2; \
            exit 1; \
        fi; \
        shared_version="$(sed -n "s/^  node${version%%.*}Version:[[:space:]]*//p" /tmp/images/constants.yml)"; \
        if [ "${shared_version}" != "${version}" ]; then \
            echo "Requested Embr Node SDK version '${version}' does not match shared version '${shared_version}'." >&2; \
            exit 1; \
        fi; \
        printf '%s\n' "${version}" >> "${manifest}"; \
    done; \
    sort -u "${manifest}" -o "${manifest}"; \
    mv "${manifest}" "/tmp/platforms/nodejs/versions/${OS_FLAVOR}/versionsToBuild.txt"; \
    bash /tmp/platforms/nodejs/getNode.sh "${OS_FLAVOR}" ""
