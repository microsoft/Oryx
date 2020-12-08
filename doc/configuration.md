# Oryx configuration

Oryx provides configuration options through environment variables so that you
can apply minor adjustments and still utilize the automatic build process. The following variables are supported today:

> NOTE: In Azure Web Apps, these variables are set as [App Settings][].

Setting name                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PRE\_BUILD\_COMMAND          | Command to a shell script to be run before build   | ""      | `echo foo`
PRE\_BUILD\_SCRIPT\_PATH     | A repo-relative path to a shell script to be run before build   | ""      | `scripts/prebuild.sh`
POST\_BUILD\_COMMAND         | Command to a shell script to be run after build    | ""      | `echo foo`
POST\_BUILD\_SCRIPT\_PATH    | A repo-relative path to a shell script to be run after build    | ""      | `scripts/postbuild.sh`
ENABLE\_DYNAMIC\_INSTALL     | Enable dynamically install platform binaries if not presented inside the image | various | `true`, `false`
ORYX\_SDK\_STORAGE\_BASE\_URL| The storage base url from where oryx dynamically install sdks | "https://oryx-cdn.microsoft.io" |
DYNAMIC\_INSTALL\_ROOT\_DIR  | Root directory path under which dynamically installed SDKs are created. | various | "/opt", "tmp/platforms/oryx"
DISABLE\_COLLECTSTATIC       | Disable running `collecstatic` when building Django apps.      | `false` | `true`, `false`
DISABLE\_CHECKERS            | Disable running version checkers during the build.             | `false` | `true`, `false`
ORYX\_DISABLE\_TELEMETRY     | Disable Oryx command line tools from collecting any data.      | `false` | `true`, `false`
ORYX\_APP\_TYPE              | Type of application that the the source directory has.         | ""  | 'functions','static-sites', 'webapps'.
DISABLE\_RECURSIVE\_LOOKUP   | Indicates if detectors should consider looking into sub-directories for files | `false` | `true`, `false`
ENABLE\_MULTIPLATFORM\_BUILD | Apply more than one toolset if repo indicates it               | `false` | `true`, `false`
DISABLE\_DOTNETCORE\_BUILD   | Do not apply .NET Core build even if repo indicates it         | `false` | `true`, `false`
DISABLE\_PYTHON\_BUILD       | Do not apply Python build even if repo indicates it            | `false` | `true`, `false`
DISABLE\_NODEJS\_BUILD       | Do not apply Node.js build even if repo indicates it           | `false` | `true`, `false`
DISABLE\_PHP\_BUILD          | Do not apply Php build even if repo indicates it         | `false` | `true`, `false`
DISABLE\_RUBY_\_BUILD        | Do not apply Ruby build even if repo indicates it         | `false` | `true`, `false`
DISABLE\_JAVA_\_BUILD        | Do not apply Java build even if repo indicates it         | `false` | `true`, `false`
DISABLE\_HUGO\_BUILD         | Do not apply Hugo build even if repo indicates it         | `false` | `true`, `false`
PLATFORM\_NAME               | Specify which platform the app is using                   | ""      | "python"
PLATFORM\_VERSION            | Specify which platform version the app is using           | ""      | "3.7.1"
REQUIRED\_OS\_PACKAGES       | Indicate if it requires OS packages for Node or Python packages | `false` | `true`, `false`
PROJECT                      | repo-relative path to directory with `.csproj` file for build  | ""      | src/WebApp1/WebApp1.csproj
MSBUILD\_CONFIGURATION       | Configuration (Debug or Relase) that is used to build a .NET Core project | `Release` | `Debug`, `Release`
ENABLE\_NODE\_MONOREPO\_BUILD| Apply node monorepo build if repo indicates it           | `false` | `true`, `false`
COMPRESS\_DESTINATION\_DIR   | Indicates if the entire output directory needs to be compressed.     | ""      | `false` | `true`, `false`
PRUNE\_DEV\_DEPENDENCIES     | Only the prod dependencies are copied to the output for Node apps. | ""      | `false` | `true`, `false`
NPM\_REGISTRY\_URL           | Specify the npm registry url.                                | ""      | "http://foobar.com/"
VIRTUALENV\_NAME             | Specify Python virtual environment name                  | ""      | "antenv2.7"

[App Settings]: https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#app-settings