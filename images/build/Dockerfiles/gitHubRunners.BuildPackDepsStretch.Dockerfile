# The digest here must match with what is present in the following location
# so that downloading of the image is very fast in GitHub workflows
# https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1804-README.md
FROM buildpack-deps:stretch@sha256:d95db8b9293c71d0f9b6a12d96d1ace65af81fd6535e5cb07078df85b3147a76
