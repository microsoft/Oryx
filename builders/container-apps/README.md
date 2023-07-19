# Container Apps Wrapper
This directory contains an image that uses the Oryx builder image as a base, but has
some additional logic added on top of it, specifically for the builds that happen within
the container apps platform.  

This extra logic is located within the [start script](./startup-script.sh), and is the container's
entrypoint.