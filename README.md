# Oryx
Builds dockers images which enable building applications of different languages in a 'build image' container 
and produces output which can be run in a 'runtime image' container.

## Getting Started
Pre-requisites
- Docker for Windows version `18.06.1-ce`. Note that this is the same version that is used in CI too. We want a  
  consistent version across development and CI agents to avoid surprises.
- Bash shell (example: Git Bash) to run build & tests scripts.

## Build and Test
Change your current working directory to the root of this repository and run these respective scripts:
- Build build images: `build/build-buildimages.sh`
- Build runtime images: `build/build-runtimeimages.sh`
- Build and test build images: `build/test-buildimages.sh`
- Build and test runtime images: `build/test-runtimeimages.sh`
- Build and test build and runtime images and other tests: `build.sh`