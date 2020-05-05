# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

function DeleteItem($pathToRemove) {
    if (Test-Path -Path $pathToRemove) {
        Remove-Item -Recurse -Force -Path "$pathToRemove"
    }
}

$version="0.2.0"
$buildScriptGeneratorName="Microsoft.Oryx.BuildScriptGenerator"
$buildScriptGeneratorCliName="Microsoft.Oryx.BuildScriptGenerator.Cli"
$repoRoot=Split-Path -parent $PSScriptRoot
$artifactsPackagesDir="$repoRoot\artifacts\packages"

cd "$artifactsPackagesDir"

# Delete any existing directory and zip file. Could have been from an earlier build.
DeleteItem "$buildScriptGeneratorName"
DeleteItem "$buildScriptGeneratorName.zip"
Rename-Item -Path "$buildScriptGeneratorName.$version.nupkg" -NewName "$buildScriptGeneratorName.zip"
Expand-Archive -Path "$buildScriptGeneratorName.zip" -DestinationPath "$buildScriptGeneratorName"
DeleteItem "$buildScriptGeneratorName.zip"

DeleteItem "$buildScriptGeneratorCliName"
DeleteItem "$buildScriptGeneratorCliName.zip"
Rename-Item -Path "$buildScriptGeneratorCliName.$version.nupkg" -NewName "$buildScriptGeneratorCliName.zip"
Expand-Archive -Path "$buildScriptGeneratorCliName.zip" -DestinationPath "$buildScriptGeneratorCliName"
DeleteItem "$buildScriptGeneratorCliName.zip"

Copy-Item `
    -Path "$repoRoot\src\BuildScriptGeneratorCli\bin\Release\linux-x64\publish\GenerateBuildScript.dll" `
    -Destination "$buildScriptGeneratorCliName\lib\netcoreapp2.1\GenerateBuildScript.dll" `
    -Force
Copy-Item `
    -Path "$repoRoot\src\BuildScriptGeneratorCli\bin\Release\linux-x64\publish\$buildScriptGeneratorName.dll" `
    -Destination "$buildScriptGeneratorCliName\lib\netcoreapp2.1\$buildScriptGeneratorName.dll" `
    -Force 
Copy-Item `
    -Path "$repoRoot\src\BuildScriptGeneratorCli\bin\Release\linux-x64\publish\Microsoft.Oryx.Common.dll" `
    -Destination "$buildScriptGeneratorCliName\lib\netcoreapp2.1\Microsoft.Oryx.Common.dll" `
    -Force 
Compress-Archive -Path "$buildScriptGeneratorCliName\*" -DestinationPath "$buildScriptGeneratorCliName.zip"
Rename-Item -Path "$buildScriptGeneratorCliName.zip" -NewName "$buildScriptGeneratorCliName.$version.nupkg"
DeleteItem "$buildScriptGeneratorCliName"

Copy-Item `
    -Path "$repoRoot\src\BuildScriptGeneratorCli\bin\Release\linux-x64\publish\$buildScriptGeneratorName.dll" `
    -Destination "$buildScriptGeneratorName\lib\netcoreapp2.1\$buildScriptGeneratorName.dll" `
    -Force 
Copy-Item `
    -Path "$repoRoot\src\BuildScriptGeneratorCli\bin\Release\linux-x64\publish\Microsoft.Oryx.Common.dll" `
    -Destination "$buildScriptGeneratorName\lib\netcoreapp2.1\Microsoft.Oryx.Common.dll" `
    -Force 
Compress-Archive -Path "$buildScriptGeneratorName\*" -DestinationPath "$buildScriptGeneratorName.zip"
Rename-Item -Path "$buildScriptGeneratorName.zip" -NewName "$buildScriptGeneratorName.$version.nupkg"
DeleteItem "$buildScriptGeneratorName"
