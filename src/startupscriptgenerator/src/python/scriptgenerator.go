// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"fmt"
	"os"
	"path/filepath"
	"strings"
)

type PythonStartupScriptGenerator struct {
	AppPath               string
	UserStartupCommand    string
	DefaultAppPath        string
	DefaultAppModule      string
	DefaultAppDebugModule string
	DebugAdapter          string // Remote debugger adapter to use. Currently, only `ptvsd` is supported.
	DebugPort             string
	DebugWait             bool // Whether debugger adapter should pause and wait for a client
	//  connection before running the app.
	BindPort                 string
	VirtualEnvName           string
	PackageDirectory         string
	SkipVirtualEnvExtraction bool
	Manifest                 common.BuildManifest
	SetupEnvironment         bool
}

const SupportedDebugAdapter = "ptvsd" // Not using an array since there's only one at the moment
const GeneratingCommandMessage = "Generating `%s` command for '%s'"

const DefaultHost = "0.0.0.0"
const DefaultBindPort = "80"

func (gen *PythonStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("python.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source.")

	pythonInstallationRoot := fmt.Sprintf("/opt/python/%s", gen.Manifest.PythonVersion)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n\n")
	if gen.SetupEnvironment {
		scriptBuilder.WriteString(fmt.Sprintf("oryx setupEnv -appPath %s\n", gen.AppPath))
	}
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.AppPath + "\n\n")

	common.SetEnvironmentVariableInScript(&scriptBuilder, "HOST", "", DefaultHost)
	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)

	scriptBuilder.WriteString(fmt.Sprintf("export PATH=\"%s/bin:${PATH}\"\n", pythonInstallationRoot))

	packageSetupBlock := gen.getPackageSetupCommand()
	scriptBuilder.WriteString(packageSetupBlock)

	appType := ""         // "Django", "Flask", etc.
	appDebugAdapter := "" // Used debugger adapter
	appDirectory := ""
	appModule := ""      // Suspected entry module in app
	appDebugModule := "" // Command to run under a debugger in case debugging mode was requested

	command := gen.UserStartupCommand // A custom command takes precedence over any framework defaults
	if command != "" {
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.AppPath)
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.AppPath)
	} else {
		var appFw PyAppFramework = DetectFramework(gen.AppPath, gen.VirtualEnvName)

		if appFw != nil {
			println("Detected an app based on " + appFw.Name())
			appType = appFw.Name()
			appDirectory = gen.AppPath
			appModule = appFw.GetGunicornModuleArg()
			appDebugModule = appFw.GetDebuggableModule()
		} else {
			println("No framework detected; using default app from " + gen.DefaultAppPath)
			logger.LogInformation("Using default app.")
			appType = "Default"
			appDirectory = gen.DefaultAppPath
			appModule = gen.DefaultAppModule
			appDebugModule = gen.DefaultAppDebugModule
		}

		if appModule != "" {
			if gen.shouldStartAppInDebugMode() {
				logger.LogInformation("Generating debug command for appDebugModule.")
				println(fmt.Sprintf(GeneratingCommandMessage, "ptvsd", appDebugModule))
				command = gen.buildPtvsdCommandForModule(appDebugModule, appDirectory)
				appDebugAdapter = gen.DebugAdapter
			} else {
				logger.LogInformation("Generating command for appModule.")
				println(fmt.Sprintf(GeneratingCommandMessage, "gunicorn", appModule))
				command = gen.buildGunicornCommandForModule(appModule, appDirectory)
			}
		}
	}

	scriptBuilder.WriteString(command + "\n")

	logger.LogProperties(
		"Finalizing script",
		map[string]string{"appType": appType, "appDebugAdapter": appDebugAdapter,
			"appModule": appModule, "venv": gen.Manifest.VirtualEnvName})

	var runScript = scriptBuilder.String()
	return runScript
}

// Builds the commands to setup the Python packages, using virtual env or a package folder.
func (gen *PythonStartupScriptGenerator) getPackageSetupCommand() string {
	scriptBuilder := strings.Builder{}

	// Values in manifest file takes precedence over values supplied at command line
	virtualEnvironmentName := gen.Manifest.VirtualEnvName
	if virtualEnvironmentName == "" {
		virtualEnvironmentName = gen.VirtualEnvName
	}

	packageDirName := gen.Manifest.PackageDir
	if packageDirName == "" {
		packageDirName = gen.PackageDirectory
	}

	if virtualEnvironmentName != "" {
		virtualEnvDir := filepath.Join(gen.AppPath, virtualEnvironmentName)

		// If virtual environment was not compressed or if it is compressed but mounted using a zip driver,
		// we do not want to extract the compressed file
		if gen.Manifest.CompressedVirtualEnvFile == "" || gen.SkipVirtualEnvExtraction {
			if common.PathExists(virtualEnvDir) {
				// We add the virtual env site-packages to PYTHONPATH instead of activating it to be backwards compatible with existing
				// app service implementation. If we activate the virtual env directly things don't work since it has hardcoded references to
				// python libraries including the absolute path. Since Python is installed in different paths in build and runtime images,
				// the libraries are not found.
				venvSubScript := getVenvHandlingScript(virtualEnvironmentName, virtualEnvDir)
				scriptBuilder.WriteString(venvSubScript)

			} else {
				packageDirName = "__oryx_packages__"
				// We just warn the user and don't error out, since we still can run the default website.
				scriptBuilder.WriteString("  echo WARNING: Could not find virtual environment directory '" + virtualEnvDir + "'.\n")
			}
		} else {
			compressedFile := gen.Manifest.CompressedVirtualEnvFile
			virtualEnvDir := "/" + virtualEnvironmentName
			if strings.HasSuffix(compressedFile, ".zip") {
				scriptBuilder.WriteString("echo Found virtual environment .zip archive.\n")
				scriptBuilder.WriteString(
					"extractionCommand=\"unzip -q " + compressedFile + " -d " + virtualEnvDir + "\"\n")

			} else if strings.HasSuffix(compressedFile, ".tar.gz") {
				scriptBuilder.WriteString("echo Found virtual environment .tar.gz archive.\n")
				scriptBuilder.WriteString(
					"extractionCommand=\"tar -xzf " + compressedFile + " -C " + virtualEnvDir + "\"\n")
			} else {
				fmt.Printf(
					"Error: Unrecognizable file '%s'. Expected a file with a '.zip' or '.tar.gz' extension.\n",
					compressedFile)
				os.Exit(consts.FAILURE_EXIT_CODE)
			}

			scriptBuilder.WriteString(
				"echo Removing existing virtual environment directory '" + virtualEnvDir + "'...\n")
			scriptBuilder.WriteString("rm -fr " + virtualEnvDir + "\n")
			scriptBuilder.WriteString("mkdir -p " + virtualEnvDir + "\n")
			scriptBuilder.WriteString("echo Extracting to directory '" + virtualEnvDir + "'...\n")
			scriptBuilder.WriteString("$extractionCommand\n")
			venvSubScript := getVenvHandlingScript(virtualEnvironmentName, virtualEnvDir)
			scriptBuilder.WriteString(venvSubScript)
		}
	}

	if packageDirName != "" {
		packageDir := filepath.Join(gen.AppPath, packageDirName)
		if common.PathExists(packageDir) {
			scriptBuilder.WriteString("echo Using package directory '" + packageDir + "'\n")
			scriptBuilder.WriteString("SITE_PACKAGE_PYTHON_VERSION=$(python -c \"import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))\")\n")
			scriptBuilder.WriteString("SITE_PACKAGES_PATH=$HOME\"/.local/lib/python\"$SITE_PACKAGE_PYTHON_VERSION\"/site-packages\"\n")
			scriptBuilder.WriteString("mkdir -p $SITE_PACKAGES_PATH\n")
			scriptBuilder.WriteString("echo \"" + packageDir + "\" > $SITE_PACKAGES_PATH\"/oryx.pth\"\n")
			scriptBuilder.WriteString("PATH=\"" + packageDir + "/bin:$PATH\"\n")
			scriptBuilder.WriteString("echo \"Updated PATH to '$PATH'\"\n")
		} else {
			// We just warn the user and don't error out, since we still can run the default website.
			scriptBuilder.WriteString("  echo WARNING: Could not find package directory '" + packageDir + "'.\n")
		}
	}

	return scriptBuilder.String()
}

func getVenvHandlingScript(virtualEnvName string, virtualEnvDir string) string {
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString(
		"PYTHON_VERSION=$(python -c \"import sys; print(str(sys.version_info.major) " +
			"+ '.' + str(sys.version_info.minor))\")\n")
	scriptBuilder.WriteString(
		"echo Using packages from virtual environment '" + virtualEnvName + "' located at '" + virtualEnvDir + "'.\n")
	virtualEnvSitePackagesDir := "\"" + virtualEnvDir + "/lib/python$PYTHON_VERSION/site-packages\""
	scriptBuilder.WriteString("export PYTHONPATH=$PYTHONPATH:" + virtualEnvSitePackagesDir + "\n")
	scriptBuilder.WriteString("echo \"Updated PYTHONPATH to '$PYTHONPATH'\"\n")
	return scriptBuilder.String()
}

// Produces the gunicorn command to run the app.
// `module` is of the pattern "<dotted module path>:<variable name>".
// The variable name refers to a WSGI callable that should be found in the specified module.
func (gen *PythonStartupScriptGenerator) buildGunicornCommandForModule(module string, appDir string) string {
	// Default to App Service's timeout value (in seconds)
	args := "--timeout 600 --access-logfile '-' --error-logfile '-'"

	if gen.BindPort != "" {
		args = appendArgs(args, "--bind="+DefaultHost+":"+gen.BindPort)
	}

	if appDir != "" {
		args = appendArgs(args, "--chdir="+appDir)
	}

	if args != "" {
		return "GUNICORN_CMD_ARGS=\"" + args + "\" gunicorn " + module
	}

	return "gunicorn " + module
}

func (gen *PythonStartupScriptGenerator) shouldStartAppInDebugMode() bool {
	logger := common.GetLogger("python.scriptgenerator.shouldStartAppInDebugMode")
	defer logger.Shutdown()

	if gen.DebugAdapter == "" {
		return false
	}

	if gen.DebugAdapter != SupportedDebugAdapter {
		logger.LogError("Unsupported debug adapter '%s'", gen.DebugAdapter)
		return false
	}

	return true
}

func (gen *PythonStartupScriptGenerator) buildPtvsdCommandForModule(moduleAndArgs string, appDir string) string {
	waitarg := ""
	if gen.DebugWait {
		waitarg = "--wait"
	}

	cdcmd := ""
	if appDir != "" {
		cdcmd = fmt.Sprintf("cd %s && ", appDir)
	}

	pycmd := fmt.Sprintf("%spython -m ptvsd --host %s --port %s %s -m %s",
		cdcmd, DefaultHost, gen.DebugPort, waitarg, moduleAndArgs)

	return cdcmd + pycmd
}

func appendArgs(currentArgs string, argToAppend string) string {
	if currentArgs != "" {
		currentArgs += " "
	}
	currentArgs += argToAppend
	return currentArgs
}
