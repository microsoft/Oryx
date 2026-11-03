# Embr Python runtime images

These images provide the runtime substrate for deployment-specific Embr
application images. They use Ubuntu Resolute and contain CPython, runtime
libraries, basic network tools, and the Ubuntu and Azure Linux certificate
trust stores.

Application frameworks, application servers, database clients, compilers,
profilers, and the Oryx startup-script generator are intentionally excluded.
Applications must package those dependencies in their build output.

EMBR Node and Python builds use the shared patch versions in
`images/constants.yml`, not duplicate pins in `images/embrconstants.yml`.

Candidate images are published
to `oryxdevmcr.azurecr.io/public/oryx/python`; release images use the existing
MCR repository:

```text
mcr.microsoft.com/oryx/python:embr-<major.minor>-ubuntu-resolute-<release>
```

On `main`, the release pipeline also publishes the moving
`embr-<major.minor>-ubuntu-resolute` tag. Consumers that require reproducible
deployments must resolve and persist the image digest.

To build and test all configured versions locally:

```bash
BUILD_DEFINITIONNAME=local \
RELEASE_TAG_NAME=dev \
bash build/buildEmbrPythonRuntimeImages.sh

bash build/testEmbrPythonRuntimeImages.sh \
  artifacts/images/embr-python-runtime-images-acr.resolute.txt
```
