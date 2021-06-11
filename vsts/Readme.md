The content in this folder is specific to any VSTS related stuff.

## Pipelines
They represent the build pipelines for this repository.
-   'validation.yml':  
    This is used for validating branches for which pull requests were sent.  
    In this pipeline both build and runtime images are built and tested but they are NOT pushed.  

-   'nightly.yml':  
    Pipeline which builds and tests build and runtime images every night.  

-   'ci.yml':  
    Pipeline which builds, tests and pushes build images and runtime images.  
    This pipeline, by default, gets triggered for check-ins into 'main' branch.  
	Also it has a scheduled build for releasing signed binaries to images on Saturday mornings.  
### PlatformBinaries
The pipelines for each platform to build and publish platform binaries to Azure Blob Storage.

## Scripts
This folder contains scripts that are used in VSTS pipelines.
