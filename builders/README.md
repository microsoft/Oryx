# Builders

This repo contains the definitions of the builder images that are used to build application source code into
runnable images.  

These builders use the buildpack ecosystem defined by the [Cloud Native Buildpacks](https://buildpacks.io/) project.
They build the application code using the Oryx project, and can use the Oryx runtime images for the final images.  

The `/base` directory contains the builder image itself, and is where the buildpack was authored. The 
`/container-apps-wrapper` directory contains the code for an image that was built on top of the builder image and 
has some additional logic, such as detecting the runtime version before running the build.
