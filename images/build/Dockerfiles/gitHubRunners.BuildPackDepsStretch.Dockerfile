# The digest here must match with what is present in the following location
# so that downloading of the image is very fast in GitHub workflows
# https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1804-README.md
FROM buildpack-deps:stretch@sha256:23c6d55a4d5ad7c9476638196920d67a97702433b5b48465496af6ab3214e7e4
