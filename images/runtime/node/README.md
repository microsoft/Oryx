# Node images

The Dockerfiles for our Node images are either produced by a template, which derives from the official image, or
created manually from the `buildpack-deps` base.

# Images derived from `buildpack-deps`

The reason we do rebuild some old images instead of relying on the official ones is security: since
the official images for older versions were made using the version of `buildpack-deps` available at the time, which
contained vulnerabilities, we rebuild them using the current version of that base image. The portion of our
Dockerfiles that install Node (which we always have in its own stage called `node-original-image`) are identical to the
ones used in the official Node image for each particular version.

# Generate images from template

Since our node images are identical except for the base image, which determines the node version,
our final node images are generated from a template file, `./template.Dockerfile`, which contains the placeholder
for the base image name. 

To generate the dockerfiles for all the images, run `./generateDockerfiles.sh`.

## Adding new versions of Node

To add a new Node version, add a new directory with the version name, and a base Dockerfile named `./base.Dockerfile`, that should contain the node binaries. The script `./generateDockerfiles.sh` will then produce the Dockerfile for the final image using the `./base.Dockerfile` image as its base.