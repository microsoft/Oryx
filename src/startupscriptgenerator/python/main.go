package main

import (
	"flag"
	"startupscriptgenerator/common"
)

func main() {
	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	defaultAppFilePathPtr := flag.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/defaultsite'")
	defaultAppModulePtr := flag.String("defaultAppModule", "application:app", "Module of the default application, e.g. 'application:app'.")
	virtualEnvironmentNamePtr := flag.String("virtualEnvName", "pythonenv", "Name of the virtual environment for the app")
	bindHostPtr := flag.String("hostBind", "0.0.0.0", "Host where the application will bind to")
	flag.Parse()

	fullAppPath := fsvalidation.GetValidatedFullPath(*appPathPtr)
	defaultAppFullPAth := fsvalidation.GetValidatedFullPath(*defaultAppFilePathPtr)

	entrypointGenerator := PythonStartupScriptGenerator{
		SourcePath:             fullAppPath,
		VirtualEnvironmentName: *virtualEnvironmentNamePtr,
		BindHost:               *bindHostPtr,
		DefaultAppPath:         defaultAppFullPAth,
		DefaultAppModule:       *defaultAppModulePtr,
	}

	command := entrypointGenerator.GenerateEntrypointCommand()
	println(command)
}
