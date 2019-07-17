**How to run**  
The `Dockerfile` in this sample does a multi-stage build where its uses the build image to do a build
and then copies the output to the runtime image. It then runs the app using the installed web server in the runtime image to launch the app.
-   Build the image: `docker build -t basicflaskappsample .`
-   Create a container: `docker run --name web -d -p 8000:5000 jamspellflaskappsample`
-   Access `http://localhost:8000` and you should see something like `Hello World!`
-   You can view logs of the running container using `docker logs <containerId>`
    
Source:
https://github.com/bakwc/JamSpell

