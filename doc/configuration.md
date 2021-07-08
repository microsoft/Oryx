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
DISABLE\_CHECKERS            | Disable running version checkers during the build.             | `false` | `true`, `false`
ORYX\_DISABLE\_TELEMETRY     | Disable Oryx command line tools from collecting any data.      | `false` | `true`, `false`
ORYX\_APP\_TYPE              | Type of application that the the source directory has.         | ""  | 'functions','static-sites', 'webapps'.
DISABLE\_RECURSIVE\_LOOKUP   | Indicates if detectors should consider looking into sub-directories for files | `false` | `true`, `false`
ENABLE\_MULTIPLATFORM\_BUILD | Apply more than one toolset if repo indicates it               | `false` | `true`, `false`
PLATFORM\_NAME               | Specify which platform the app is using                   | ""      | "python"
PLATFORM\_VERSION            | Specify which platform version the app is using           | ""      | "3.7.1"
REQUIRED\_OS\_PACKAGES       | Indicate if it requires OS packages for Node or Python packages | `false` | `true`, `false`
CREATE\_PACKAGE              | Indicate if it shoud create packages for the app          | `false` | `true`, `false`

Setting name for .NET apps   | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
DOTNET\_VERSION              | Specify which .NET version the app is using                    | ""      | "5.0.100"
DISABLE\_DOTNETCORE\_BUILD   | Do not apply .NET Core build even if repo indicates it         | `false` | `true`, `false`
PROJECT                      | repo-relative path to directory with `.csproj` file for build  | ""      | "src/WebApp1/WebApp1.csproj"
MSBUILD\_CONFIGURATION       | Configuration (Debug or Relase) that is used to build a .NET Core project | `Release` | `Debug`, `Release`

Setting name for Nodejs apps | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
NODE\_VERSION                | Specify which Node version the app is using                    | ""      | "14.15.0"
DISABLE\_NODEJS\_BUILD       | Do not apply Node.js build even if repo indicates it           | `false` | `true`, `false`
CUSTOM_BUILD_COMMAND         | Custom build command to be run to build Node app               | ""  | "npm ci"
RUN_BUILD_COMMAND            | Custom run build command to be run after package install commands  | ""  | "npm run build"
ENABLE\_NODE\_MONOREPO\_BUILD| Apply node monorepo build if repo indicates it                 | `false` | `true`, `false`
COMPRESS\_DESTINATION\_DIR   | Indicates if the entire output directory needs to be compressed.   | ""      | `false` | `true`, `false`
PRUNE\_DEV\_DEPENDENCIES     | Only the prod dependencies are copied to the output for Node apps. | ""      | `false` | `true`, `false`
NPM\_REGISTRY\_URL           | Specify the npm registry url.                                | ""      | "http://foobar.com/"

Setting name for Python apps | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PYTHON\_VERSION              | Specify which Python version the app is using                  | ""      | "2.7.1"
DISABLE\_PYTHON\_BUILD       | Do not apply Python build even if repo indicates it            | `false` | `true`, `false`
VIRTUALENV\_NAME             | Specify Python virtual environment name                        | ""      | "antenv2.7"
DISABLE\_COLLECTSTATIC       | Disable running `collecstatic` when building Django apps.      | `false` | `true`, `false`

Setting name for Php apps    | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PHP\_VERSION                 | Specify which Php version the app is using                     | ""      | "7.4"
PHP\_COMPOSER\_VERSION       | Specify which Php composer version the app is using            | ""      | "7.2.15"
DISABLE\_PHP\_BUILD          | Do not apply Php build even if repo indicates it         | `false` | `true`, `false`

Setting name for Java apps | Description                                                    | Default | Example
---------------------------|----------------------------------------------------------------|---------|----------------
JAVA\_VERSION              | Specify which Java version the app is using                    | ""      | "14.0.2"
MAVEN\_VERSION             | Specify which Maven version the app is using                   | ""      | "3.6.3"
DISABLE\_JAVA_\_BUILD      | Do not apply Java build even if repo indicates it              | `false` | `true`, `false`

Setting name for Ruby apps | Description                                                    | Default | Example
---------------------------|----------------------------------------------------------------|---------|----------------
RUBY\_VERSION              | Specify which Ruby version the app is using                    | ""      | "2.7.1"
DISABLE\_RUBY_\_BUILD      | Do not apply Ruby build even if repo indicates it              | `false` | `true`, `false`

Setting name for Hugo apps | Description                                                    | Default | Example
---------------------------|----------------------------------------------------------------|---------|----------------
HUGO\_VERSION              | Specify which Hugo version the app is using                    | ""      | "0.76.3"
DISABLE\_HUGO\_BUILD       | Do not apply Hugo build even if repo indicates it              | `false` | `true`, `false`



[App Settings]: https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#app-settings



# Oryx-generated manifest file

`oryx-manifest.toml` is the orxy-generated manifest file involved in 2 stages (build & run). 
First the build stage will build the application and autogenerate `oryx-manifest.toml`.
Secondly `oryx-manifest.toml` is used by the startup script at runtime to setup Docker container variables. The following are the supported platforms and their key/values:

> NOTE: In Azure Web Apps, these variables are set as [App Settings]

Node variant manifest file                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
NodeVersion          | Platform's version identified by Oryx   | ""      | "14.15.1"
OutputDirPath          |  Output is in separate volume due to rsync errors  |      | ".nuxt"
OperationId          | Used to correlate with logs. Which logs??   | "|lTpVCq2KGuY=.e6f14074_"      |
SourceDirectoryInBuildContainer          | Directory inside Docker container contaiing source code??   |     | "/tmp/sampleapps/nodejs/helloworld-nuxtjs/" 
PlatformName          | Name of Oryx supported platform name   | ""      | "nodejs"
CompressDestinationDir          | Determines whether artifact is compressed; allows decompression to occur. To save memory??   | ""      | "false"

Python variant manifest file                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PythonVersion          | Platform's version identified by Oryx   | ""      | "3.8.6"
virtualEnvName          |  Name of the virtual environmen running in the Docker container  |      | "pythonenv3.8"
OperationId          | Used to correlate with logs. Which logs??   | "|lTpVCq2KGuY=.e6f14074_"      |
SourceDirectoryInBuildContainer          | Directory inside Docker container contaiing source code??   |     | "/tmp/sampleapps/python/flask-app/"
PlatformName          | Name of Oryx supported platform name   | ""      | "python"
CompressDestinationDir          | Determines whether artifact is compressed; allows decompression to occur. To save memory??   | ""      | "false"

Dotnetcore variant manifest file                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
DotNetCoreSdkVersion          | Platform's version identified by Oryx   | ""      | "3.1.406"
StartupDllFileName          | Executable used to run the application |      | "NetCoreApp31.MvcApp.dll"
OperationId          | Used to correlate with logs. Which logs??   | "|lTpVCq2KGuY=.e6f14074_"      |
SourceDirectoryInBuildContainer          | Directory inside Docker container contaiing source code??   |     | "/tmp/sampleapps/DotNetCore/NetCoreApp31.MvcApp/"
PlatformName          | Name of Oryx supported platform name   | ""      | "python"
CompressDestinationDir          | Determines whether artifact is compressed; allows decompression to occur. To save memory??   | ""      | "false"