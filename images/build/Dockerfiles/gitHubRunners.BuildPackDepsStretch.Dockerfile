# The digest here must match with what is present in the following location
# so that downloading of the image is very fast in GitHub workflows
# https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1804-README.md
FROM buildpack-deps:stretch@sha256:dc901bbf4b34e4ca8771c0d0773e557221452f97bcf0c732de7ecda3782bdf97 AS main
