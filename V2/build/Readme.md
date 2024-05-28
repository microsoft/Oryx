## Why so many scripts?
The scripts in this folder are broken down into different layers to enable a good development as a well as CI experience.  
So if a user wants to just build and test build images, they can do so by running its respective script.  
These scripts are actually called in VSTS pipelines too (Check the `vsts/pipelines/templates/_buildTemplate.yml` file).

## Artifact files
Building build and runtime images can conceptually be viewed as building a .NET repo's 'src' folder where after successfully  
building the output is written to a artifacts folder. In a similar way, after building build/runtime images, we write out   
the names of the images that were built to files '/artifacts/images/build-images.txt' and '/artifacts/images/runtime-images.txt'.  
This is the same experience if the images were built locally on a dev machine or a CI agent. The idea is that in case of CI agent,  
when a VSTS task needs to push images to a docker registry, it can just consume these artifact files to figure out which images to push.

## Untagging images
Taggig a docker image is actually tagging or pinning docker layers. Since a CI agent can go through several builds a day we would be  
in the risk of running out of disk space. So we want to make sure to not pin layers which are not used anymore. 
For example,  
Let's say the following docker file was built with 2 tags called 'test:latest' and 'test:buildnumber1'. Let's call the individual  
layers as L1, L2 and L3.
```
FROM busybox                        <-- L1
RUN echo hello > hello.txt          <-- L2
RUN echo hi > hi.txt                <-- L3
```

Now let's say this docker file was changed in the following way and a new docker build is done with 2 tags called 'test:latest'  
and 'test:**buildnumber2**'. When the following docker file is built there would be new layers called L22 and L33.
```
FROM busybox                        <-- L1
RUN echo hello-updated > hello.txt  <-- L22
RUN echo hi > hi.txt                <-- L33
```

At this point the 'latest' and 'buildnumber2' tags are pointing to L33 layer and ideally we do not want to keep the layers L2 and L3  
around as they are not going to be used anyway and just consume disk space. But they are still pinned via the tag 'buildnumber1'. 

So, to prevent this we try to unpin/untag a build number based tag (not the 'latest' tag because its a *moving* tag) which will  
create dangling layers/images helping us to cleanup (for example, L2 and L3 would no longer have any references and can be cleaned up).

## Why tag intermediate stages in the multi-stage build image dockerfile?
As of docker version `18.06.1-ce`, whenever docker builds a multi-stage dockerfile, it only tags the layer of the final FROM related  
image. Since intermediate FROM statements exist in a multi-stage process, their layers are left dangled. This is problematic for us  
because we, in general, want to clean out dangling images (for example, frequent changes to BuildScriptGenerator tool results in   
dangling images which we want to clean) to avoid running into disk space problems. So, to prevent these dangling intermediate FROM  
statement related images from being cleaned up (i.e rebuilding the whole stage again from scratch), we tag them. These images are  
however never pushed to the server and only exist on the machine where the image was built.
