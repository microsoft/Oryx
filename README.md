# Oryx

[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/Oryx/Oryx-BuildImage?branchName=master)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=9922?branchName=master)

Oryx is a build system which automatically compiles source code repos into
runnable artifacts.

Oryx generates and runs an opinionated build script based on analysis of the
repo's contents; for example, if `package.json` is discovered in the repo
Oryx includes `npm run build` in the build script; if `requirements.txt` is
found it includes `pip install -r requirements.txt`. Oryx also analyzes and
selects a run-time entry point for the app such as `npm run start` for
Node.js or a WSGI host for Python.

# Supported runtimes

Runtime | Version
--------|--------
Python  | 2.7<br />3.6,3.7
Node.js | 4.4,4.5,4.8<br />6.2,6.6,6.9,6.10,6.11<br />8.0,8.1,8.2,8.8,8.9,8.11,8.12<br />9.4<br />10.1,10.12,10.14

Patches (0.0.**x**) are applied as soon as possible after they are released upstream.

# Use the system yourself

Though primarily intended for use within Azure services, you can also use the
Oryx build system yourself for troubleshooting and tests. Following are
simple instructions; for more details see [our docs](./doc).

Oryx includes two command-line applications; the first is included in the
build image and generates a build script. The second is included in run
images and generates a run script. Both are aliased and accessible as `oryx`
in their respective environments.

## Build (`oryx build`)

* `build`: Generate and run build scripts.
* `script`: Generate build script and print to stdout.
* `languages`: Show the list of supported languages and versions.

For all options, specify `oryx --help`.

The build command takes the parameter `--output` to specify where prepared
artifacts will be placed; if not specified the source directory is used for
output as well.

## Run (`oryx -appPath`)

The Oryx application in the run image generates a start script named run.sh, by
default in the same folder as the compiled artifact.

## Build and run an app

To build and run an app from a repo, follow these steps. An example script
follows.

1. Mount the repo's directory as a volume in Oryx's "build" container.
1. Run `oryx build ...` within the container to build a runnable artifact.
1. Mount the output directory from build in one of Oryx's "run" containers.
1. Run `oryx --appPath ...` within the container to write a start script.
1. Run the generated started script, by default `./run.sh`.

From your locally-cloned repo, run the following. Be sure to add
`-p/--publish` and `-e/--env` flags to the "run" docker command as necessary.

```bash
# build
docker run --volume $(pwd):/repo \
    'mcr.microsoft.com/oryx/build:latest' \
    'oryx build /repo --output /repo/out'

# run
docker run --detach --rm \
    --volume $(pwd)/out:/app \
    # use --publish to expose ports
    # --publish 8080:8080 \
    # use --env to add env vars
    # --env MYKEY=value \
    'mcr.microsoft.com/oryx/node-10.12:latest' \
    # or use the Python image
    # 'mcr.microsoft.com/oryx/python-3.7:latest \
    sh -c 'oryx -appPath /app && /app/run.sh'
```

# Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md).

# License

MIT, see [LICENSE.md](./LICENSE.md).

# Security

Security issues and bugs should be reported privately, via email, to the
Microsoft Security Response Center (MSRC) at
[secure@microsoft.com](mailto:secure@microsoft.com). You should receive a
response within 24 hours. If for some reason you do not, please follow up via
email to ensure we received your original message. Further information,
including the [MSRC
PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found
in the [Security
TechCenter](https://technet.microsoft.com/en-us/security/default).

# Data/Telemetry

When utilized within Azure services, this project collects usage data and
sends it to Microsoft to help improve our products and services. Read
[Microsoft's privacy statement][] to learn more.

[Microsoft's privacy statement]: http://go.microsoft.com/fwlink/?LinkId=521839

This project follows the [Microsoft Open Source Code of Conduct][coc]. For
more information see the [Code of Conduct FAQ][cocfaq]. Contact
[opencode@microsoft.com][cocmail] with questions and comments.

[coc]: https://opensource.microsoft.com/codeofconduct/
[cocfaq]: https://opensource.microsoft.com/codeofconduct/faq/
[cocmail]: mailto:opencode@microsoft.com
