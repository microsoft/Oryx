FROM buildpack-deps:curl

RUN apt-get update && apt-get install -y --no-install-recommends \
		xz-utils \
	&& rm -rf /var/lib/apt/lists/*