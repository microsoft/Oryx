# Oryx Automation

This is a simple command-line tool built using C# and the .NET framework,
that automates the deployment of platforms 

This tool is intended to be ran in [GitHub Actions workflow](https://github.com/microsoft/Oryx/actions/workflows/automationTemplate.yaml). 
Triggered on a [cron](https://github.com/microsoft/Oryx/actions/workflows/automationTrigger.yaml).
Please see `Oryx/.github/workflows` for source code.
## Prerequisites

To use this tool, you must have the following installed:

- .NET Core 3.1

## Getting Started

To use the tool, follow the steps below:

1. Clone the Oryx repository
2. Run `dotnet run --project build/tools/Automation/Automation.csproj {platformName} {absoluteOryxRootPath}`
3. Notice the `build/constants.yaml` and corresponding `versionsToBuild.txt` files get updated, after step2.

## Configuration
The supported GitHub action variables can be configured [here](https://github.com/microsoft/Oryx/settings/variables/actions),
which get passed down to the Gihub Action workflow.

Setting name                 | Description                                                    | Default | Example
-----------------------------|----------------------------------------------------------------|---------|----------------
PYTHON_MIN_RELEASE_VERSION   | Python's minimum release semantic version using (inclusive)	  |  ""     | "3.10.9"
PYTHON_MAX_RELEASE_VERSION   | Python's maximum release semantic version using (inclusive)	  |  ""     | "3.10.9"
PYTHON_BLOCKED_VERSIONS_ARRAY| Python's blocked semantic versions. Delimited by comma.		  |  ""     | "3.10.9, 3.10.10"
DOTNET_MIN_RELEASE_VERSION   | DotNet's minimum release semantic version using (inclusive)	  |  ""     | "3.10.9"
DOTNET_MAX_RELEASE_VERSION   | DotNet's maximum release semantic version using (inclusive)	  |  ""     | "3.10.9"
DOTNET_BLOCKED_VERSIONS_ARRAY| DotNet's blocked semantic versions. Delimited by comma.		  |  ""     | "3.10.9, 3.10.10"
