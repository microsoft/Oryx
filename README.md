# Oryx Build System

The Oryx build system compiles a code repo into runnable artifacts. It
generates and runs an opinionated build script based on analysis of the repo's
contents; for example if it discovers `package.json` it includes `npm run
build` in the build script. Currently supported runtimes are listed in the
following section.

The system depends on many build-time tools like compilers and header files. It
should be used within the "build" image defined in `./images/build`.

Built artifacts are intended to be used with the runtime images also defined
here in `./images/runtime`.

## Supported runtimes

Runtime | Version
--------|--------
Python  | 2.7<br />3.6,3.7
Node.js | 4.4,4.5,4.8<br />6.2,6.6,6.9,6.10,6.11<br />8.0,8.1,8.2,8.8,8.9,8.11,8.12<br />9.4<br />10.1,10.12,10.13

# Using the system

Use the builder: `docker run oryxdevms/build oryx --help`.

Currently supported commands include:

* build: Generate and run build scripts.
* languages: Show the list of supported languages and their versions.
* script: Generate build script and print to stdout.

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
