# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# In this script, we extract already created nuget packages, which were created by dotnet cli automatically
# but do not have signed dlls within them.
# So we extract the package and update it's contents with signed dlls and repackage it.
# The steps which signs the nuget package itself would come after this step in DevOps pipeline.

function DeleteItem($pathToRemove) {
    if (Test-Path -Path $pathToRemove) {
        Remove-Item -Recurse -Force -Path "$pathToRemove"
    }
}

$repoRoot="$PSScriptRoot\..\.."
$artifactsPackagesDir="$repoRoot\artifacts\packages"
. $repoRoot\build\detector\__detectorNugetPackagesVersions.ps1
$detectorName="Microsoft.Oryx.Detector"
$commonProjectAssemblyName="Microsoft.Oryx.Common"
cd "$artifactsPackagesDir"

# Delete any existing directory and zip file. Could have been from an earlier build.
DeleteItem "$detectorName"
DeleteItem "$detectorName.zip"
Rename-Item -Path "$detectorName.$VERSION.nupkg" -NewName "$detectorName.zip"
Expand-Archive -Path "$detectorName.zip" -DestinationPath "$detectorName"
DeleteItem "$detectorName.zip"

Copy-Item `
    -Path "$repoRoot\src\Detector\bin\Release\netcoreapp2.1\$detectorName.dll" `
    -Destination "$detectorName\lib\netcoreapp2.1\$detectorName.dll" `
    -Force
Copy-Item `
    -Path "$repoRoot\src\Detector\bin\Release\netcoreapp2.1\$commonProjectAssemblyName.dll" `
    -Destination "$detectorName\lib\netcoreapp2.1\$commonProjectAssemblyName.dll" `
    -Force

Compress-Archive -Path "$detectorName\*" -DestinationPath "$detectorName.zip"
Rename-Item -Path "$detectorName.zip" -NewName "$detectorName.$VERSION.nupkg"
DeleteItem "$detectorName"
