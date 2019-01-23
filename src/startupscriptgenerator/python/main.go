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

	logger := common.GetLogger("python.scriptgenerator.main")
	println("Session Id : " + logger.SessionId)

	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	userStartupCommandPtr := flag.String("userStartupCommand", "", "[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := flag.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/defaultsite'")
	defaultAppModulePtr := flag.String("defaultAppModule", "application:app", "Module of the default application, e.g. 'application:app'.")
	virtualEnvironmentNamePtr := flag.String("virtualEnvName", "", "Name of the virtual environment for the app")
	packagesFolderPtr := flag.String("packagedir", "__oryx_packages__", "Directory where the python packages were installed, if no virtual environment was used.")
	bindHostPtr := flag.String("hostBind", "0.0.0.0", "Host where the application will bind to")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	flag.Parse()

	fullAppPath := common.GetValidatedFullPath(*appPathPtr)
	defaultAppFullPAth := common.GetValidatedFullPath(*defaultAppFilePathPtr)

	entrypointGenerator := PythonStartupScriptGenerator{
		SourcePath:             fullAppPath,
		UserStartupCommand:     *userStartupCommandPtr,
		VirtualEnvironmentName: *virtualEnvironmentNamePtr,
		BindHost:               *bindHostPtr,
		DefaultAppPath:         defaultAppFullPAth,
		DefaultAppModule:       *defaultAppModulePtr,
		PackageDirectory:       *packagesFolderPtr,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, command)
}
