FROM buildpack-deps:stretch-curl

RUN apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y --no-install-recommends \
		xz-utils \
	&& rm -rf /var/lib/apt/lists/*

COPY images/receivePgpKeys.sh /tmp/scripts/receivePgpKeys.sh
RUN chmod +x /tmp/scripts/receivePgpKeys.sh
