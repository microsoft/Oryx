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

# Azure App Service configuration

Within Azure App Service, Oryx's environment variables are set via [App
Settings][].

List and modify these App Settings with the [az CLI][] using the following
commands:

```bash
app_group=your-group
app_name=your-app

# list current settings
az webapp config appsettings list \
  --resource-group $app_group --name $app_name \
  --output table

# replace current settings
az webapp config appsettings set \
    --resource-group $app_group --name $app_name \
    --settings \
      "settingA=${settingA}" \
      "settingB=${settingB}"
```

App Service adds the following settings that govern build:

Setting name                        | Description                                                    | Example
------------------------------------|----------------------------------------------------------------|------------
COMMAND                             | provide an alternate build-and-run script. Bypasses automatic build completely. | "repo/path/to/script.sh"
ENABLE\_ORYX\_BUILD                 | if `true`, use the Oryx build system instead of the legacy Kudu system | 
SCM\_DO\_BUILD\_DURING\_DEPLOYMENT` | if `false`, bypass automatic build |

## Startup file

Within App Service, to explicitly specify a start script use the
`--startup-file` parameter of `az webapp create ...` or `az webapp config set
...`.

[App Settings]: https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#app-settings
[az CLI]: https://github.com/Azure/azure-cli
