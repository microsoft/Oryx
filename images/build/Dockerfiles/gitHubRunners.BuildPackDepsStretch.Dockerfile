# The digest here must match with what is present in the following location
# so that downloading of the image is very fast in GitHub workflows
# https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1804-README.md
FROM buildpack-deps:stretch@sha256:dee4275fc056551e1f83c5a3ea024510ca63f03ceedd9a1c29cbab70644b046b
