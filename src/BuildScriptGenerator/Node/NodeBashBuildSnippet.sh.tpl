echo Installing packages ...
cd "$SOURCE_DIR"
echo
echo "Running '{{ PackageInstallCommand }}' ..."
echo

{{ PackageInstallCommand }}

{{ NpmRunBuildCommand }}

{{ NpmRunBuildAzureCommand }}
