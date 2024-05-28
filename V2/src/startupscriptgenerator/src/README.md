# Startup Script Generator

## Setup
Setting up go on your local machine to update dependencies in these modules
1. [Install go](https://go.dev/doc/install) onto your local machine

## Updating dependencies
1. The [go.mod file](https://go.dev/doc/modules/gomod-ref) within each directory describes the module’s properties, including its dependencies on other modules and on versions of Go
2. These dependencies can locked in using the `replace` key word
3. Copy the `common` directory into your `$GOROOT/src` directory. You can find where this is by attempting to run `go build` in any `go.mod` directory besides `common`. This needs to be done since all other go modules import the `common` and `common/consts` modules.
4. Run `go mod tidy` wherever the dependencies have been updated

## Testing
1. Navigate to the `Oryx/build` directory
2. Execute the `./testStartupScriptGenerators.sh` script from inside that directory