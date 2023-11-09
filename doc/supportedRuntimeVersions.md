# Supported Runtime Versions for Oryx

Below are the supported runtime versions for Oryx that can be pulled via Microsoft Container Registry following the format
`mcr.microsoft.com/oryx/{platform}:{version}`.

_Note_: the `{version}` used as the tag of the runtime image corresponds to the latest minor _or_ patch version supported by Oryx, _i.e._, `node:8`
corresponds to the latest version of node `8.x.x` supported by Oryx, and `node:8.0` corresponds to the latest version of node `8.0.x` supported.

The full list of supported platform versions for Oryx can be found [here](supportedPlatformVersions.md).

## .NET

The following are supported runtime versions for the image `mcr.microsoft.com/oryx/dotnetcore:{version}`.

- `1.0`
- `1.1`
- `2.0`
- `2.1`
- `2.2`
- `3.0`
- `3.1`
- `3.2`
- `5.0`
- `6.0`
- `7.0`

## Node

The following are supported runtime versions for the image `mcr.microsoft.com/oryx/node:{version}`.

- `4.4`
- `4.5`
- `4.8`
- `6`
- `6.2`
- `6.6`
- `6.9`
- `6.10`
- `6.11`
- `8`
- `8.0`
- `8.1`
- `8.2`
- `8.8`
- `8.9`
- `8.11`
- `8.12`
- `8.16`
- `9.4`
- `10`
- `10.1`
- `10.10`
- `10.12`
- `10.14`
- `10.16`
- `10.17`
- `12`
- `12.7`
- `12.8`
- `12.9`
- `12.12`
- `12.13`
- `14`
- `16`

## PHP

The following are supported runtime versions for the image `mcr.microsoft.com/oryx/php:{version}`.

- `5.6`
- `7.0`
- `7.2`
- `7.3`
- `7.4`
- `8.0`
- `8.1`
- `8.2`

## Python

The following are supported runtime versions for the image `mcr.microsoft.com/oryx/python:{version}`.

- `2.7`
- `3.6`
- `3.7`
- `3.8`
- `3.9`
- `3.10`
- `3.11`
- `3.12`

## Ruby

The following are supported runtime versions for the image `mcr.microsoft.com/oryx/ruby:{version}`.

- `2.5`
- `2.6`
- `2.7`



# Oryx runtime base images debian flavor

Oryx runtime images have different debian flavor and gradually migrate older debian flavor images like Stretch to newer debian flavor Bullseye images.
The information won't show in runtime image tags, but we can always check the debian version by checking the environment variable DEBIAN_FLAVOR within that image.

## Stretch based runtime images

### dotnet
- 1.0, 1.1, 2.0, 2.1, 2.2

### nodejs
- 4.4, 4.5, 4.8, 6, 6.2, 6.6, 6.9, 6.10, 6.11, 8, 8.0, 8.1, 8.2, 8.8, 8.9, 8.11, 8.12, 9.4, 10, 10.1, 10.10, 10.12, 10.14, 12

### php
- 5.6, 7.0, 7.2, 7.3

### php-fpm
- 5.6, 7.0, 7.2, 7.3

### python
- 2.7, 3.6


## Buster based runtime images

### dotnet
- 3.0, 5.0, 6.0, 7.0

### php
- 8.0

### php-fpm
- 8.0

### python
- 3.9

### ruby
- 2.5, 2.6, 2.7


## Bullseye based runtime images

### dotnet
- 3.1

### php
- 8.1, 8.2

### php-fpm
- 8.1, 8.2

### nodejs
- 14, 16, 18

### python
- 3.7, 3.8, 3.10, 3.11