# Generate images from template

Since our node images are identical except for the base image, which determines the node version,
all our node images are generated from a template file, `Dockerfile.template`, which contains the a placefolder for the base image name. File `nodeVersions.txt` has a list of the supported node versions as their tags for the official
node images.

To generate the dockerfiles for all the images, run `generateDockerfiles.sh`.

## Adding new versions of Node

To add a new Node version, simply add a line to `nodeVersions.txt`, or update the patch version of one of the
existing ones, and run the script `generateDockerfiles.sh`. This script already computes the target folder as well,
and creates it if it doesn't exist.