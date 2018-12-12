# Oryx Build System

The Oryx build system compiles a code repo into runnable artifacts. It
generates and runs an opinionated build script based on analysis of the repo's
contents; for example if it discovers `package.json` it includes `npm run
build` in the build script. Currently supported runtimes are listed [below](#supportedRuntimes).

The system is divided in two sets of images: build, which includes the SDKs for all platforms,
and runtime, which are much smaller in size and are specific to a given language and version.
Both sets of images are made to interact through a file sharing mechanism, either using a network volume,
or by copying the output of the command ran in the build image into the runtime-base image using a Dockerfile.

The "build" image is defined in `./images/build`, and its development bits are pushed to Docker repository `oryxdevms/build`; it contains many build-time tools like compilers and header files. 

The runtime images are defined in `./images/runtime`, and their development images are listed in
[https://hub.docker.com/r/oryxdevms/]. They also contain a tool that detects how the app should be started by
analyzing the build output directory, and that tool can be found under `/opt/startupcmdgen/startupcmdgen` inside
each image. They output a startup script to a file that then will run the app when executed.

## <a name="supportedRuntimes">Supported runtimes

Runtime | Version
--------|--------
Python  | 2.7<br />3.6,3.7
Node.js | 4.4,4.5,4.8<br />6.2,6.6,6.9,6.10,6.11<br />8.0,8.1,8.2,8.8,8.9,8.11,8.12<br />9.4<br />10.1,10.12,10.14.1

# Using the system

To build an app, mount it as a volume inside the build container, and run our build tool using the `oryx` command. For details further details, run `docker run  oryx --help`.

Currently supported commands include:

* build: Generate and run build scripts.
* languages: Show the list of supported languages and their versions.
* script: Generate build script and print to stdout.

The build command accepts an optional output directory where the compiled bits will be placed, and if none is provided they will be added in the source directory itself. This directory can then be volume mounted in the runtime
image corresponding to the language and version being used by the app. Using the startup detection tool, the app can 
be started from there, for example using `docker run -v <path to source>:/app oryxdevms/python-3.7 bash -c "/opt/startupcmdgen/startupcmdgen -appPath /app -output /app/start.sh && /app/start.sh"`.

# License

MIT, see [LICENSE.md](./LICENSE.md).

# Contributing

See [CONTRIBUTING.md](./contributing.md).

This project follows the [Microsoft Open Source Code of Conduct][coc]. For more
information see the [Code of Conduct FAQ][cocfaq]. Contact
[opencode@microsoft.com][cocmail] with questions and comments.

[coc]: https://opensource.microsoft.com/codeofconduct/
[cocfaq]: https://opensource.microsoft.com/codeofconduct/faq/
[cocmail]: mailto:opencode@microsoft.com
