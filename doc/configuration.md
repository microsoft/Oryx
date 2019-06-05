# Oryx configuration

Oryx provides configuration options through environment variables so that you
can apply minor adjustments and still utilize the automatic build process. The following variables are supported today:

> NOTE: In Azure Web Apps, these variables are set as App Settings [App Settings][].

Setting name                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PRE\_BUILD\_COMMAND          | Command or a repo-relative path to a shell script to be run before build   | ""      | `echo foo`, `scripts/prebuild.sh`
POST\_BUILD\_COMMAND         | Command or a repo-relative path to a shell script to be run after build    | ""      | `echo foo`, `scripts/postbuild.sh`
DISABLE\_COLLECTSTATIC       | Disable running `collecstatic` when building Django apps.      | `false` | `true`, `false`
PROJECT                      | repo-relative path to directory with `.csproj` file for build  | ""      | src/WebApp1/WebApp1.csproj
ENABLE\_MULTIPLATFORM\_BUILD | apply more than one toolset if repo indicates it               | `false` | `true`, `false`
DISABLE\_DOTNETCORE\_BUILD   | do not apply .NET Core build even if repo indicates it         | `false` | `true`, `false`
DISABLE\_PYTHON\_BUILD       | do not apply Python build even if repo indicates it            | `false` | `true`, `false`
DISABLE\_NODEJS\_BUILD       | do not apply Node.js build even if repo indicates it           | `false` | `true`, `false`
MSBUILD\_CONFIGURATION       | Configuration (Debug or Relase) that is used to build a .NET Core project | `Release` | `Debug`, `Release`
