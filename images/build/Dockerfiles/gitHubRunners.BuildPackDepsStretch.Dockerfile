# The digest here must match with what is present in the following location
# so that downloading of the image is very fast in GitHub workflows
# https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1804-README.md
FROM buildpack-deps:stretch@sha256:34a18637ed801407f7a17a29575e82264fb0818f9b6a0c890f8a6530afea43dc
