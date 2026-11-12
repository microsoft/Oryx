# Node runtime images

The active runtime Dockerfiles are checked in under each major version and OS:
20 and 22 use Bullseye/Bookworm, 24 uses Noble, and 26 uses Resolute.
Their common base and Node-specific base are stages in the same Dockerfile.
No separate base build, external `BASE_IMAGE`, or Dockerfile generator is required.

Build using the repository root as the Docker context. Supply exact Node and
tooling versions from `images/constants.yml`, and provide the configured package
feed credential using the `npmrc` BuildKit secret.

The runtime retains signed Node download verification and the Go startup
generator. The Resolute profile has its own dependencies and Yarn installation;
it does not inherit the classic Node base.

## Adding a major version

Declare its exact version and supported OS list in `images/constants.yml`, add
the corresponding runtime Dockerfile, and select it in the runtime pipeline.
Reuse the matching multi-stage profile where behavior is unchanged; preserve
intentional OS, startup-tooling, and user-ID differences.