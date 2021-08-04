FROM mcr.microsoft.com/oryx/base:dotnetcore-2.0-20210628.1

# Older .NET core versions, which have reached end of life and therefore are no longer updated, use
# a version of `curl` that has known issues.
# We manually update it here so we can still depend on the original images.
# This command should be removed once support for deprecated .NET core images is halted.
RUN sed -i '/jessie-updates/d' /etc/apt/sources.list  # Now archived

RUN apt-get update \
  && apt-get install -y \
     curl \
     file \
     openssl \
     libgdiplus \
  && apt-get upgrade -y \
  && rm -rf /var/lib/apt/lists/*
  