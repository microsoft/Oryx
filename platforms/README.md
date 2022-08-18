## How to add a new release version and publish to Azure Blob Storage?
Add a new version, its SHA, and GPG keys information to `{language}/versions/{ostype}/versionsToBuild.txt` file for any operating systems that you would like the version to be available. Update version constants in `constants.yaml` and other files.
If it's a default version, reset `defaultVersion.txt` file.
Manually trigger the platform binary build pipeline after sent PR and update related tests.

# Options to overwrite platform binaries when publishing SDKs to dev or prod storage account
We set up environment variable during the pipeline execution. When trigger a new build in Oryx-PlatformBinary-<PlatformName> in Azure DevOps pipelines, click the 'run pipeline'->'Advanced options'->'Variables' and add the variable name and value there.
Set up OVERWRITE_EXISTING_SDKS="true" to overwrite SDKs in all platforms.
Set up OVERWRITE_EXISTING_SDKS_<PlatformName>="true" to overwrite SDKs in a specific platform.
