Reference: [_Create a stack_](https://buildpacks.io/docs/operator-guide/create-a-stack/)

## Create the base image

```
cd .\oryx-builder\stack
docker build . -t oryx/sample-stack-base:skeleton --target base
```

## Create the run image

```
cd .\oryx-builder\stack
docker build . -t oryx/sample-stack-run:skeleton --target run
```

## Create the build image

```
cd .\oryx-builder\stack
docker build . -t oryx/sample-stack-build:skeleton --target build
```

## Create all three stack images

```
cd .\oryx-builder\stack
docker build . -t oryx/sample-stack-base:skeleton --target base
docker build . -t oryx/sample-stack-run:skeleton --target run
docker build . -t oryx/sample-stack-build:skeleton --target build
```