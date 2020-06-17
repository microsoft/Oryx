# Folders in the image which we use to build this image itself
# These are deleted in the final stage of the build
ARG DEBIAN_FLAVOR
ARG IMAGES_DIR=/tmp/oryx/images
ARG BUILD_DIR=/tmp/oryx/build
# Determine where the image is getting built (DevOps agents or local)
ARG AGENTBUILD

# NOTE: This imge is NOT based on 'githubrunners-buildpackdeps-stretch' because AzFunctions
# team wanted a consistent experience at their end and do not want to see latency that might
# be caused due to when the GitHub runners' layers and Oryx image layers go out of sync.
FROM githubrunners-buildpackdeps-${DEBIAN_FLAVOR} AS main
ARG BUILD_DIR
ARG IMAGES_DIR
ARG DEBIAN_FLAVOR

# Configure locale (required for Python)
# NOTE: Do NOT move it from here as it could have global implications
ENV LANG C.UTF-8

# Install basic build tools
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

RUN echo "debian flavor is: ${DEBIAN_FLAVOR}" ;\
    if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu63 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* ; \
    else \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu57 \
            liblttng-ust0 \
            libssl1.0.2 \
        && rm -rf /var/lib/apt/lists/* ; \
    fi

# A temporary folder to hold all content temporarily used to build this image.
# This folder is deleted in the final stage of building this image.
RUN mkdir -p ${IMAGES_DIR}
RUN mkdir -p ${BUILD_DIR}
ADD build ${BUILD_DIR}
ADD images ${IMAGES_DIR}
# chmod all script files
RUN find ${IMAGES_DIR} -type f -iname "*.sh" -exec chmod +x {} \;
RUN find ${BUILD_DIR} -type f -iname "*.sh" -exec chmod +x {} \;

# This is the folder containing 'links' to benv and build script generator
RUN mkdir -p /opt/oryx

# Install Node.js, NPM, Yarn
FROM main AS node-install
ARG BUILD_DIR
ARG IMAGES_DIR
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        jq \
        curl \
        ca-certificates \
        gnupg \
        dirmngr \
    && rm -rf /var/lib/apt/lists/*
RUN ${IMAGES_DIR}/build/installHugo.sh
COPY build/__nodeVersions.sh /tmp/scripts
RUN cd ${IMAGES_DIR} \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ./installPlatform.sh nodejs $NODE12_VERSION
RUN ${IMAGES_DIR}/build/installNpm.sh
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ${IMAGES_DIR}/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5 \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
 && curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
 && gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
 && mkdir -p /opt/yarn \
 && tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn \
 && mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION \
 && rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz

RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $NODE12_VERSION /opt/nodejs/12 \
 && ln -s 12 /opt/nodejs/lts
RUN set -ex \
 && . ${BUILD_DIR}/__nodeVersions.sh \
 && ln -s $YARN_VERSION /opt/yarn/stable \
 && ln -s $YARN_VERSION /opt/yarn/latest \
 && ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION \
 && ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION
RUN set -ex \
 && mkdir -p /links \
 && cp -s /opt/nodejs/lts/bin/* /links \
 && cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links

FROM main AS final
ARG BUILD_DIR
ARG IMAGES_DIR
ARG SDK_STORAGE_ENV_NAME
ARG SDK_STORAGE_BASE_URL_VALUE
WORKDIR /

ENV ORYX_PATHS="/opt/oryx:/opt/nodejs/lts/bin:/opt/yarn/stable/bin:/opt/hugo/lts"
ENV PATH="${ORYX_PATHS}:$PATH"
COPY images/build/benv.sh /opt/oryx/benv
RUN chmod +x /opt/oryx/benv

# Copy NodeJs, NPM and Yarn related content
COPY --from=node-install /opt /opt

# Build script generator content. Docker doesn't support variables in --from
# so we are building an extra stage to copy binaries from correct build stage
COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx

RUN rm -rf /tmp/oryx

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
ENV ENABLE_DYNAMIC_INSTALL=true
ENV ${SDK_STORAGE_ENV_NAME} ${SDK_STORAGE_BASE_URL_VALUE}

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}

ENTRYPOINT [ "benv" ]
