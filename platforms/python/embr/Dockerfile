ARG BASE_IMAGE
FROM ${BASE_IMAGE}
ARG OS_FLAVOR=resolute
ENV OS_FLAVOR=${OS_FLAVOR}
ARG VERSIONS_TO_BUILD_OVERRIDE=""
ENV VERSIONS_TO_BUILD_OVERRIDE=${VERSIONS_TO_BUILD_OVERRIDE}

RUN . /etc/os-release; \
    if [ "${OS_FLAVOR}" != "resolute" ] || [ "${ID}" != "ubuntu" ] || [ "${VERSION_CODENAME}" != "resolute" ]; then \
        echo "Embr Python SDK builds require an Ubuntu Resolute base and OS_FLAVOR=resolute." >&2; \
        exit 1; \
    fi

COPY platforms/python/prereqs/build.sh /tmp/build.sh
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/build.sh /tmp/receiveGpgKeys.sh \
    && apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ca-certificates \
        wget \
        gnupg \
        dirmngr \
        xz-utils \
        build-essential \
        tk-dev \
        uuid-dev \
        libgeos-dev

COPY platforms /tmp/platforms
COPY build /tmp/build
COPY images/constants.yml /tmp/images/constants.yml
RUN mkdir -p /tmp/sdk

# Resolve all rows before replacing only this disposable container's OS manifest.
# App Service keeps using platforms/python/Dockerfile and the unchanged buildPython.sh.
RUN set -eu; \
    manifest="$(mktemp)"; \
    remaining="${VERSIONS_TO_BUILD_OVERRIDE},"; \
    while [ -n "${remaining}" ]; do \
        version="$(printf '%s' "${remaining%%,*}" | xargs)"; \
        remaining="${remaining#*,}"; \
        minor="$(printf '%s' "${version}" | cut -d. -f1,2)"; \
        metadata="$(bash /tmp/build/getEmbrPythonVersionMetadata.sh "${minor}")"; \
        if [ "${metadata%%|*}" != "${version}" ]; then \
            echo "Requested Embr Python SDK version '${version}' does not match shared version '${metadata%%|*}'." >&2; \
            exit 1; \
        fi; \
        printf '%s,\n' "${metadata}" | tr '|' ',' >> "${manifest}"; \
    done; \
    sort -u "${manifest}" -o "${manifest}"; \
    mv "${manifest}" "/tmp/platforms/python/versions/${OS_FLAVOR}/versionsToBuild.txt"; \
    bash /tmp/platforms/python/buildPython.sh "${OS_FLAVOR}" ""
