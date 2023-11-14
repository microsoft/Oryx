Reference: [_Building blocks of a Cloud Native Buildpack_](https://buildpacks.io/docs/buildpack-author-guide/create-buildpack/building-blocks-cnb/)

## Package buildpack

Reference the [`packaged-buildpack`](../packaged-buildpack) folder for more information on how to package the buildpack.

## Set default builder

```
pack config default-builder cnbs/sample-builder:bionic
```

## Trust default builder

```
pack config trusted-builders add cnbs/sample-builder:bionic
```

## Build the buildpack

```
pack build test-python-app --path ./python-sample-app --buildpack ./buildpack
```