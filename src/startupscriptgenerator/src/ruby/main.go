// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"flag"
	"fmt"
	"os"
	"strconv"
	"strings"
)

func main() {
	versionCommand := flag.NewFlagSet(consts.VersionCommandName, flag.ExitOnError)

	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)
	setupEnvAppPathPtr := setupEnvCommand.String(
		"appPath",
		".",
		"The path to the application folder, e.g. '/home/site/wwwroot/'.")

	scriptCommand := flag.NewFlagSet(consts.CreateScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String(
		"appPath",
		".",
		"The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")
	userStartupCommandPtr := scriptCommand.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := scriptCommand.String(
		"defaultApp",
		"",
		"[Optional] Path to a default file that will be executed if the entrypoint is not found. "+
			"Ex: '/opt/startup/default-static-site.js'")
	bindPortPtr := scriptCommand.String(
		"bindPort",
		"",
		"[Optional] Port where the application will bind to. Default is 8080")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	
	flag.Parse()

	logger := common.GetLogger("ruby.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	commands := []*flag.FlagSet{versionCommand, scriptCommand, setupEnvCommand}
	common.ValidateCommands(commands)

	if scriptCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*appPathPtr)
		fullDefaultAppFilePath := common.GetValidatedFullPath(*defaultAppFilePathPtr)

		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)

		fullOutputPath := ""
		if *outputPathPtr != "" {
			// NOTE: This path might not exist, so do not try to validate it yet.
			fullOutputPath, _ = filepath.Abs(*outputPathPtr)
		}
		var configuration Configuration
		viperConfig := common.GetViperConfiguration(fullAppPath)
		configuration.EnableDynamicInstall = viperConfig.GetBool(consts.EnableDynamicInstallKey)
		configuration.PreRunCommand = viperConfig.GetString(consts.PreRunCommandEnvVarName)

		gen := RubyStartupScriptGenerator{
			SourcePath:                      fullAppPath,
			UserStartupCommand:              *userStartupCommandPtr,
			DefaultAppFilePath:              fullDefaultAppFilePath,
			BindPort:                        *bindPortPtr,
			Manifest:                        buildManifest,
			Configuration:                   configuration,
		}
		script := gen.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, script)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)

		rubyInstallationRoot := fmt.Sprintf(
			"%s/%s",
			consts.RubyInstallationRootDir,
			buildManifest.RubyVersion)
		script := common.GetSetupScript(
			"ruby",
			buildManifest.RubyVersion,
			rubyInstallationRoot)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString("echo Installing dependencies...\n")
		scriptBuilder.WriteString("/opt/ruby/installDependencies.sh\n")
		finalScript := scriptBuilder.String()
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'Ruby' version '%s'...\n",
			buildManifest.RubyVersion))
		common.SetupEnv(finalScript)
	}
}
