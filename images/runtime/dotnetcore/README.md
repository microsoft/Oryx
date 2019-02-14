# Generate images from template

Since most of our .NET Core images are identical except for the base image, they are generated from a template file, `./Dockerfile.template`, which contains the placeholder for the base image name. File `./dotnetCoreVersions.txt` has a list of the supported .NET Core versions as their tags for the official .NET Core images.

To generate the dockerfiles for the images that are based on the template, run `./generateDockerfiles.sh`.

## Adding new versions of .NET Core

To add a new .NET Core version, simply add a line to `./dotnetCoreVersions.txt`, or update the patch version of one of the
existing ones, and run the script `./generateDockerfiles.sh`. This script already computes the target folder as well,
and creates it if it doesn't exist.