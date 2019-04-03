// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"startupscriptgenerator/common"
)

func main() {
	common.PrintVersionInfo()

	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	userStartupCommandPtr := flag.String("userStartupCommand", "", "[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := flag.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/defaultsite'")
	defaultAppModulePtr := flag.String("defaultAppModule", "application:app", "Module of the default application, e.g. 'application:app'.")
	virtualEnvironmentNamePtr := flag.String("virtualEnvName", "", "Name of the virtual environment for the app")
	packagesFolderPtr := flag.String("packagedir", "__oryx_packages__", "Directory where the python packages were installed, if no virtual environment was used.")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 80")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	flag.Parse()

	fullAppPath := common.GetValidatedFullPath(*appPathPtr)
	defaultAppFullPAth := common.GetValidatedFullPath(*defaultAppFilePathPtr)

	common.SetGlobalOperationId(fullAppPath)

	buildManifest := common.GetBuildManifest(fullAppPath)
	if buildManifest.ZipAllOutput == "true" {
		srcFolder := fullAppPath
		destFolder := "/tmp/output"

		common.ExtractZippedOutput(srcFolder, destFolder)

		// Update the variables so the downstream code uses these updated paths
		fullAppPath = destFolder
	}

	entrypointGenerator := PythonStartupScriptGenerator{
		SourcePath:             fullAppPath,
		UserStartupCommand:     *userStartupCommandPtr,
		VirtualEnvironmentName: *virtualEnvironmentNamePtr,
		BindPort:               *bindPortPtr,
		DefaultAppPath:         defaultAppFullPAth,
		DefaultAppModule:       *defaultAppModulePtr,
		PackageDirectory:       *packagesFolderPtr,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, command)
}
