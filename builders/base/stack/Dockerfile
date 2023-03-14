# 1. Set a common base
FROM mcr.microsoft.com/oryx/cli:builder-debian-buster-20230208.1 as base

# 2. Set required CNB information
ENV CNB_USER_ID=1000
ENV CNB_GROUP_ID=1000
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

# 3. Create the user
RUN groupadd cnb --gid ${CNB_GROUP_ID} && \
  useradd --uid ${CNB_USER_ID} --gid ${CNB_GROUP_ID} -m -s /bin/bash cnb

# 4. Install common packages
RUN apt-get update && \
  apt-get install -y xz-utils ca-certificates && \
  rm -rf /var/lib/apt/lists/*

# 5. Start a new run stage
FROM base as run

# 6. Set user and group (as declared in base image)
USER ${CNB_USER_ID}:${CNB_GROUP_ID}

# 7. Start a new build stage
FROM base as build

ENV ORYX_SDK_STORAGE_BASE_URL="https://oryx-cdn.microsoft.io"
ENV ENABLE_DYNAMIC_INSTALL="true"

# 8. Install packages that we want to make available at build time
RUN apt-get update && \
  apt-get install -y git wget jq && \
  rm -rf /var/lib/apt/lists/* && \
  wget https://github.com/sclevine/yj/releases/download/v5.0.0/yj-linux -O /usr/local/bin/yj && \
  chmod +x /usr/local/bin/yj

USER root

RUN chown ${CNB_USER_ID}:${CNB_GROUP_ID} /opt

# 9. Set user and group (as declared in base image)
USER ${CNB_USER_ID}:${CNB_GROUP_ID}