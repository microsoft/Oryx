// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"flag"
)

func main() {
	// setup flags
	versionCommand := flag.NewFlagSet(consts.VersionCommandName, flag.ExitOnError)
	
	// setup env commands
	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)

	// setup script commands
	scriptCommand := flag.NewFlagSet(consts.CreateScriptCommandName, flag.ExitOnError)
	defaultAppFilePathPtr := scriptCommand.String("defaultApp", "", "[Optional] Path to a default file that will be "+
		"executed if the entrypoint is not found. Ex: '/opt/defaultsite'")
	appPathPtr := scriptCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 80")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")
	userStartupCommandPtr := scriptCommand.String("userStartupCommand", "", "[Optional] Command that will be executed "+
		"to start the application up.")
	
	logger := common.GetLogger("golang.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	commands := []*flag.FlagSet{versionCommand, scriptCommand, setupEnvCommand}
	common.ValidateCommands(commands)
	if scriptCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*appPathPtr)
		defaultAppFullPath := common.GetValidatedFullPath(*defaultAppFilePathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)
		var configuration Configuration
		viperConfig := common.GetViperConfiguration(fullAppPath)
		configuration.EnableDynamicInstall = viperConfig.GetBool(consts.EnableDynamicInstallKey)
		configuration.PreRunCommand = viperConfig.GetString(consts.PreRunCommandEnvVarName)
		
		entrypointGenerator := GolangStartupScriptGenerator{
			AppPath:                  fullAppPath,
			UserStartupCommand:       *userStartupCommandPtr,
			BindPort:                 *bindPortPtr,
			DefaultAppPath:           defaultAppFullPath,
			Manifest:                 buildManifest,
			Configuration:            configuration,
		}
		command := entrypointGenerator.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, command)
	}
}
