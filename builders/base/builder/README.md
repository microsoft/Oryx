Reference: [_Create a builder_](https://buildpacks.io/docs/operator-guide/create-a-builder/)

## Create builder

The following must have been built first:

- [Stack](../stack)
- [Buildpack](../buildpack)

```
cd .\oryx-builder\builder
pack builder create oryx-builder:bionic --config .\builder.toml
```

## Use builder

```
pack build my-app my-builder:bionic --path .\oryx\tests\SampleApps\<path_to_app>
```

## Running the app

```
docker run --rm --entrypoint sys-info -it my-app
```