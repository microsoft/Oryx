Reference: [_Package a buildpack_](https://buildpacks.io/docs/buildpack-author-guide/package-a-buildpack/)

## Package the buildpack as an image

The following must have been built first:

- [Stack](../stack)

```
cd .\oryx-builder\packaged-buildpack
pack buildpack package oryx-buildpack --config .\package.toml
```