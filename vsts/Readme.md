The content in this folder is specific to any VSTS related stuff.

## Pipelines
They represent the build pipelines for this repository.
-   'validation.yml':  
    This is used for validating branches for which pull requests were sent.  
    In this pipeline both build and runtime images are built and tested but they are NOT pushed.  

-   'runtime.yml':  
    Pipeline which builds, tests and pushes runtime images.  
    This pipeline, by default, gets triggered for check-ins into 'master' branch.  

-   'buildimage.yml':  
    Pipeline which builds, tests and pushes build images.  
    This pipeline, by default, gets triggered for check-ins into 'master' branch.  

## Scripts
This folder contains scripts that are used in VSTS pipelines.
