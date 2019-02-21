# specify with `--build-arg "runtime-version"`
# available runtimes listed at https://hub.docker.com/u/oryxprod
ARG RUNTIME=node-10.14

FROM docker.io/oryxprod/build:latest as build
WORKDIR /app
COPY . .
RUN oryx build /app

FROM docker.io/oryxprod/${RUNTIME}:latest
COPY --from=build /app /app
RUN cd /app && oryx
ENTRYPOINT ["/app/run.sh"]