# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

function DeleteItem($pathToRemove) {
    if (Test-Path -Path $pathToRemove) {
        Remove-Item -Recurse -Force -Path "$pathToRemove"
    }
}

$repoRoot="$PSScriptRoot\..\.."
$artifactsPackagesDir="$repoRoot\artifacts\packages"
. $repoRoot\build\__detectorNugetPackagesVersions.ps1
$detectorName="Microsoft.Oryx.Detector"
cd "$artifactsPackagesDir"

# Delete any existing directory and zip file. Could have been from an earlier build.
DeleteItem "$detectorName"
DeleteItem "$detectorName.zip"
Rename-Item -Path "$detectorName.$VERSION.nupkg" -NewName "$detectorName.zip"
Expand-Archive -Path "$detectorName.zip" -DestinationPath "$detectorName"
DeleteItem "$detectorName.zip"

Copy-Item `
    -Path "$repoRoot\src\Detector\bin\Release\netstandard2.0\$detectorName.dll" `
    -Destination "$detectorName\lib\netstandard2.0\$detectorName.dll" `
    -Force

Compress-Archive -Path "$detectorName\*" -DestinationPath "$detectorName.zip"
Rename-Item -Path "$detectorName.zip" -NewName "$detectorName.$VERSION.nupkg"
DeleteItem "$detectorName"
