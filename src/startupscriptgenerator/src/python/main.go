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
	"path/filepath"
	"strings"
)

func main() {
	versionCommand := flag.NewFlagSet(consts.VersionCommandName, flag.ExitOnError)

	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)
	setupEnvAppPathPtr := setupEnvCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")

	scriptCommand := flag.NewFlagSet(consts.CreateScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")
	userStartupCommandPtr := scriptCommand.String("userStartupCommand", "", "[Optional] Command that will be executed "+
		"to start the application up.")
	defaultAppFilePathPtr := scriptCommand.String("defaultApp", "", "[Optional] Path to a default file that will be "+
		"executed if the entrypoint is not found. Ex: '/opt/defaultsite'")
	defaultAppModulePtr := scriptCommand.String("defaultAppModule", "application:app", "Module of the default application,"+
		" e.g. 'application:app'.")
	defaultAppDebugModulePtr := scriptCommand.String("defaultAppDebugModule", "application.py", "Module to run if "+
		"running the app in debug mode, e.g. 'application.py start_dev_server'. Has no effect if -debugAdapter isn't used.")
	debugAdapterPtr := scriptCommand.String("debugAdapter", "", "Python debugger adapter. Currently, only 'debugpy' is supported.")
	debugPortPtr := scriptCommand.String("debugPort", "5678", "Port where the debugger will bind to. Has no effect if -debugAdapter isn't used.")
	debugWaitPtr := scriptCommand.Bool("debugWait", false, "Whether the debugger adapter should pause and wait for a "+
		"client connection before running the app.")
	virtualEnvNamePtr := scriptCommand.String("virtualEnvName", "", "Name of the virtual environment for the app")
	packagesFolderPtr := scriptCommand.String("packagedir", "", "Directory where the python packages were installed, if "+
		"no virtual environment was used.")
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 80")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	skipVirtualEnvExtraction := scriptCommand.Bool(
		"skipVirtualEnvExtraction",
		false,
		"Disables the extraction of the compressed virtual environment file. If used, some external tool will "+
			"have to extract it - otherwise the application might not work.")

	logger := common.GetLogger("python.main")
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

		entrypointGenerator := PythonStartupScriptGenerator{
			AppPath:                  fullAppPath,
			UserStartupCommand:       *userStartupCommandPtr,
			VirtualEnvName:           *virtualEnvNamePtr,
			BindPort:                 *bindPortPtr,
			DefaultAppPath:           defaultAppFullPath,
			DefaultAppModule:         *defaultAppModulePtr,
			DefaultAppDebugModule:    *defaultAppDebugModulePtr,
			DebugAdapter:             *debugAdapterPtr,
			DebugPort:                *debugPortPtr,
			DebugWait:                *debugWaitPtr,
			PackageDirectory:         *packagesFolderPtr,
			SkipVirtualEnvExtraction: *skipVirtualEnvExtraction,
			Manifest:                 buildManifest,
			Configuration:            configuration,
		}

		command := entrypointGenerator.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, command)

		userRunCommand := common.ParseUserRunCommand(filepath.Join(fullAppPath, consts.AppSvcFileName))
		common.AppendScript(*outputPathPtr, userRunCommand)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		pythonInstallationRoot := fmt.Sprintf(
			"%s/%s",
			consts.PythonInstallationRootDir,
			buildManifest.PythonVersion)
		script := common.GetSetupScript(
			"python",
			buildManifest.PythonVersion,
			pythonInstallationRoot)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString("echo Installing dependencies...\n")
		scriptBuilder.WriteString(
			fmt.Sprintf("echo %s/lib > /etc/ld.so.conf.d/python.conf\n", pythonInstallationRoot))
		scriptBuilder.WriteString("ldconfig\n")

		if strings.HasPrefix(buildManifest.PythonVersion, "3.") && !strings.HasPrefix(buildManifest.PythonVersion, "3.11") {
			scriptBuilder.WriteString(
				fmt.Sprintf("cd %s/bin\n", pythonInstallationRoot))
			scriptBuilder.WriteString("rm -f python\n")
			scriptBuilder.WriteString("ln -s python3 python\n")
			scriptBuilder.WriteString("ln -s idle3 idle\n")
			scriptBuilder.WriteString("ln -s pydoc3 pydoc\n")
			scriptBuilder.WriteString("ln -s python3-config python-config\n")
		}
		// To enable following pip commands to run, set the path env variable
		scriptBuilder.WriteString(
			fmt.Sprintf("export PATH=\"%s/bin:${PATH}\"\n", pythonInstallationRoot))
		scriptBuilder.WriteString("pip install --upgrade pip\n")
		scriptBuilder.WriteString("pip install gunicorn\n")
		scriptBuilder.WriteString("pip install debugpy\n")
		scriptBuilder.WriteString("echo Done installing dependencies.\n")
		finalScript := scriptBuilder.String()
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'python' version '%s'...\n",
			buildManifest.PythonVersion))
		common.SetupEnv(finalScript)
	}
}
