FROM oryxdevmcr.azurecr.io/public/oryx/build AS main
ARG SDK_STORAGE_ENV_NAME
ARG SDK_STORAGE_BASE_URL_VALUE
ARG AI_KEY
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

COPY build/__condaConstants.sh /tmp/__condaConstants.sh
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        apt-transport-https \
    && rm -rf /var/lib/apt/lists/*
RUN curl https://repo.anaconda.com/pkgs/misc/gpgkeys/anaconda.asc | gpg --dearmor > conda.gpg
RUN install -o root -g root -m 644 conda.gpg /usr/share/keyrings/conda-archive-keyring.gpg
RUN gpg --keyring /usr/share/keyrings/conda-archive-keyring.gpg --no-default-keyring --fingerprint 34161F5BF5EB1D4BFBBB8F0A8AEB4F8B29D82806
RUN echo "deb [arch=amd64 signed-by=/usr/share/keyrings/conda-archive-keyring.gpg] https://repo.anaconda.com/pkgs/misc/debrepo/conda stable main" > /etc/apt/sources.list.d/conda.list
RUN . /tmp/__condaConstants.sh \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        conda=${CONDA_VERSION} \
    && rm -rf /var/lib/apt/lists/*
ENV CONDA_SCRIPT="/opt/conda/etc/profile.d/conda.sh"
RUN . $CONDA_SCRIPT && \
    conda config --add channels conda-forge && \
    conda config --set channel_priority strict && \
    conda config --set env_prompt '({name})' && \
    echo "source ${CONDA_SCRIPT}" >> ~/.bashrc

RUN mkdir -p /opt/oryx/conda
COPY images/build/python/conda/ /opt/oryx/conda
    
RUN rm -rf /tmp/oryx

ENV PATH="$ORIGINAL_PATH:$ORYX_PATHS"
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
ENV ${SDK_STORAGE_ENV_NAME} ${SDK_STORAGE_BASE_URL_VALUE}
ENV ORYX_PREFER_USER_INSTALLED_SDKS=true
LABEL com.microsoft.oryx.git-commit=${GIT_COMMIT}
LABEL com.microsoft.oryx.build-number=${BUILD_NUMBER}
LABEL com.microsoft.oryx.release-tag-name=${RELEASE_TAG_NAME}