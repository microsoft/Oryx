## How to add a new release version and publish to Azure Blob Storage?
Add a new version and its SHA, GPG keys information to `versionsToBuild.txt` file. Update version constants in `constants.yaml` and other files.
If it's a default version, reset `defaultVersion.txt` file.
Manually trigger the platform binary build pipeline after sent PR and update related tests.