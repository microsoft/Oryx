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
	"runtime"
	"strconv"
	"strings"
)

type PythonStartupScriptGenerator struct {
	AppPath               string
	UserStartupCommand    string
	DefaultAppPath        string
	DefaultAppModule      string
	DefaultAppDebugModule string
	DebugAdapter          string // Remote debugger adapter to use.
	DebugPort             string
	DebugWait             bool // Whether debugger adapter should pause and wait for a client
	//  connection before running the app.
	BindPort                 string
	VirtualEnvName           string
	PackageDirectory         string
	SkipVirtualEnvExtraction bool
	Manifest                 common.BuildManifest
	Configuration            Configuration
}

const GeneratingCommandMessage = "Generating `%s` command for '%s'"

const DefaultHost = "0.0.0.0"
const DefaultBindPort = "80"

func getSupportedDebugAdapters() []string {
	return []string{"debugpy"}
}

func (gen *PythonStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("python.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source.")

	var pythonVersion string
	if gen.Manifest.PythonVersion != "" {
		pythonVersion = gen.Manifest.PythonVersion
	} else {
		pythonVersion = os.Getenv("PYTHON_VERSION")
	}
	pythonInstallationRoot := fmt.Sprintf("/opt/python/%s", pythonVersion)

	common.PrintVersionInfo()

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n\n")

	if gen.Manifest.CompressDestinationDir == "true" {
		println("Output is compressed. Extracting it...")
		
		// Try lz4 first (best compression/speed ratio)
		tarballLz4 := filepath.Join(gen.AppPath, "output.tar.lz4")
		if common.PathExists(tarballLz4) {
			println("Found output.tar.lz4, extracting...")
			common.ExtractTarball(tarballLz4, gen.Manifest.SourceDirectoryInBuildContainer)
			println(fmt.Sprintf("App path is set to '%s'", gen.Manifest.SourceDirectoryInBuildContainer))
		} else {
			// Try zstd next
			tarballZst := filepath.Join(gen.AppPath, "output.tar.zst")
			if common.PathExists(tarballZst) {
				println("Found output.tar.zst, extracting...")
				common.ExtractTarball(tarballZst, gen.Manifest.SourceDirectoryInBuildContainer)
				println(fmt.Sprintf("App path is set to '%s'", gen.Manifest.SourceDirectoryInBuildContainer))
			} else {
				// Fallback to gzip
				tarballGz := filepath.Join(gen.AppPath, "output.tar.gz")
				if common.PathExists(tarballGz) {
					println("Found output.tar.gz, extracting...")
					common.ExtractTarball(tarballGz, gen.Manifest.SourceDirectoryInBuildContainer)
					println(fmt.Sprintf("App path is set to '%s'", gen.Manifest.SourceDirectoryInBuildContainer))
				} else {
					panic("No compressed output file found (tried .lz4, .zst, .gz)")
				}
			}
		}
	}
	scriptBuilder.WriteString(fmt.Sprintf("echo 'export APP_PATH=\"%s\"' >> ~/.bashrc\n", gen.getAppPath()))
	scriptBuilder.WriteString("echo 'cd $APP_PATH' >> ~/.bashrc\n")

	if gen.Configuration.EnableDynamicInstall && !common.PathExists(pythonInstallationRoot) {
		scriptBuilder.WriteString(fmt.Sprintf("oryx setupEnv -appPath %s\n", gen.getAppPath()))
	}

	common.SetupPreRunScript(&scriptBuilder, gen.getAppPath(), gen.Configuration.PreRunCommand)

	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.getAppPath() + "\n\n")
	scriptBuilder.WriteString("export APP_PATH=\"" + gen.getAppPath() + "\"\n")

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
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.getAppPath())
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.getAppPath())
	} else {
		var appFw PyAppFramework = DetectFramework(gen.getAppPath(), gen.VirtualEnvName)

		if appFw != nil {
			println("Detected an app based on " + appFw.Name())
			appType = appFw.Name()
			appDirectory = gen.getAppPath()
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
			// Patch all legacy ptvsd debug adaptor calls to debugpy
			if gen.DebugAdapter == "ptvsd" {
				gen.DebugAdapter = "debugpy"
			}

			if gen.shouldStartAppInDebugMode() {
				logger.LogInformation("Generating debug command for appDebugModule.")
				println(fmt.Sprintf(GeneratingCommandMessage, gen.DebugAdapter, appDebugModule))
				switch gen.DebugAdapter {
				case "debugpy":
					command = gen.buildDebugPyCommandForModule(appDebugModule, appDirectory)
				}

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
		virtualEnvDir := filepath.Join(gen.getAppPath(), virtualEnvironmentName)
		virtualEnvPath := gen.getVirtualEnvPath(virtualEnvDir, virtualEnvironmentName)

		scriptBuilder.WriteString(
			fmt.Sprintf("echo 'export VIRTUALENVIRONMENT_PATH=\"%s\"' >> ~/.bashrc\n", virtualEnvPath))
		

		// If virtual environment was not compressed or if it is compressed but mounted using a zip driver,
		// we do not want to extract the compressed file
		if gen.Manifest.CompressedVirtualEnvFile == "" || gen.SkipVirtualEnvExtraction {
			scriptBuilder.WriteString(fmt.Sprintf("echo 'if [ -f %s/bin/activate ]; then . %s/bin/activate; fi' >> ~/.bashrc\n", virtualEnvPath, virtualEnvPath))
			if common.PathExists(virtualEnvDir) {
				// We add the virtual env site-packages to PYTHONPATH instead of activating it to be backwards compatible with existing
				// app service implementation. If we activate the virtual env directly things don't work since it has hardcoded references to
				// python libraries including the absolute path. Since Python is installed in different paths in build and runtime images,
				// the libraries are not found.
				venvSubScript := gen.getVenvHandlingScript(virtualEnvironmentName, virtualEnvDir)
				scriptBuilder.WriteString(venvSubScript)

			} else {
				packageDirName = "__oryx_packages__"
				// We just warn the user and don't error out, since we still can run the default website.
				scriptBuilder.WriteString("  echo WARNING: Could not find virtual environment directory '" + virtualEnvDir + "'.\n")
			}
		} else {
			scriptBuilder.WriteString(fmt.Sprintf("echo '. /%s/bin/activate' >> ~/.bashrc\n", virtualEnvironmentName))
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
			venvSubScript := gen.getHandleVenvPresentInRootScript(virtualEnvDir, virtualEnvironmentName)
			scriptBuilder.WriteString(venvSubScript)
			venvHandlingScript := gen.getVenvHandlingScript(virtualEnvironmentName, virtualEnvDir)
			scriptBuilder.WriteString(venvHandlingScript)
		}
	}

	if packageDirName != "" {
		packageDir := filepath.Join(gen.getAppPath(), packageDirName)
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

func (gen *PythonStartupScriptGenerator) getVenvHandlingScript(virtualEnvName string, virtualEnvDir string) string {
	scriptBuilder := strings.Builder{}

	// We install 'gunicorn' and 'ptvsd' when building the runtime images. Since they get installed in 'global' scope
	// here we are trying to update the python path so that 'gunicorn' and 'ptvsd' know about the site packages which
	// are part of virtual environment of the app too.
	scriptBuilder.WriteString(
		"PYTHON_VERSION=$(python -c \"import sys; print(str(sys.version_info.major) " +
			"+ '.' + str(sys.version_info.minor))\")\n")
	scriptBuilder.WriteString(
		"echo Using packages from virtual environment '" + virtualEnvName + "' located at '" + virtualEnvDir + "'.\n")
	virtualEnvSitePackagesDir := "\"" + virtualEnvDir + "/lib/python$PYTHON_VERSION/site-packages\""
	scriptBuilder.WriteString("export PYTHONPATH=$PYTHONPATH:" + virtualEnvSitePackagesDir + "\n")
	scriptBuilder.WriteString("echo \"Updated PYTHONPATH to '$PYTHONPATH'\"\n")

	if gen.Manifest.CompressDestinationDir == "true" {
		scriptBuilder.WriteString(fmt.Sprintf(". %s/bin/activate\n", virtualEnvName))
	} else {
		// When pip install is run with virtual environment option in build container, it causes the scripts within the
		// virtual environment folder to have hard-coded paths to source directory structure of the build container.
		// Here we are trying to mimic that structure so that activation of the virtual environment in runtime container
		// does not fail.
		if gen.Manifest.CompressedVirtualEnvFile == "" || gen.SkipVirtualEnvExtraction {
			if gen.Manifest.SourceDirectoryInBuildContainer != "" {
				scriptBuilder.WriteString("mkdir -p " + gen.Manifest.SourceDirectoryInBuildContainer + "\n")
				scriptBuilder.WriteString(
					fmt.Sprintf("ln -sf %s %s\n", virtualEnvDir, gen.Manifest.SourceDirectoryInBuildContainer))
				scriptBuilder.WriteString(fmt.Sprintf(". %s/bin/activate\n", virtualEnvName))
			}
		} else {
			scriptBuilder.WriteString(fmt.Sprintf(". /%s/bin/activate\n", virtualEnvName))
		}
	}

	return scriptBuilder.String()
}

// Produces the gunicorn command to run the app.
// `module` is of the pattern "<dotted module path>:<variable name>".
// The variable name refers to a WSGI callable that should be found in the specified module.
func (gen *PythonStartupScriptGenerator) buildGunicornCommandForModule(module string, appDir string) string {
	// Default to App Service's timeout value (in seconds)
	args := "--timeout 600 --access-logfile '-' --error-logfile '-'"

	pythonUseGunicornConfigFromPath := os.Getenv(consts.PythonGunicornConfigPathEnvVarName)
	if pythonUseGunicornConfigFromPath != "" {
		args = appendArgs(args, "-c "+pythonUseGunicornConfigFromPath)
	}

	pythonEnableGunicornMultiWorkers := common.GetBooleanEnvironmentVariable(consts.PythonEnableGunicornMultiWorkersEnvVarName)

	if pythonEnableGunicornMultiWorkers {
		// One worker will be reading or writing from the socket while the other worker is processing a request.
		// For MWMT (Multi Worker Multi Thread), user specifies two environment variables to enable MWMT.
		// Otherwise, this script will use the recommended setting by Gunicorn.
		pythonCustomWorkerNum := os.Getenv(consts.PythonGunicornCustomWorkerNum)
		pythonCustomThreadNum := os.Getenv(consts.PythonGunicornCustomThreadNum)
		workers := ""
		if pythonCustomWorkerNum != "" {
			workers = pythonCustomWorkerNum
		} else {
			workers = strconv.Itoa((2 * runtime.NumCPU()) + 1)
			// 2N+1 number of workers is recommended by Gunicorn docs.
			// Where N is the number of CPU threads.
		}
		args = appendArgs(args, "--workers="+workers)
		if pythonCustomThreadNum != "" {
			args = appendArgs(args, "--threads="+pythonCustomThreadNum)
		}
	}

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

	isSupported := false
	for _, adapter := range getSupportedDebugAdapters() {
		if gen.DebugAdapter == adapter {
			isSupported = true
		}
	}
	if !isSupported {
		logger.LogError("Unsupported debug adapter '%s'", gen.DebugAdapter)
		return false
	}

	return true
}

func (gen *PythonStartupScriptGenerator) buildDebugPyCommandForModule(moduleAndArgs string, appDir string) string {
	waitarg := ""
	if gen.DebugWait {
		waitarg = "--wait-for-client"
	}

	cdcmd := ""
	if appDir != "" {
		cdcmd = fmt.Sprintf("cd %s && ", appDir)
	}

	pycmd := fmt.Sprintf("%spython -m debugpy --listen %s:%s %s -m %s",
		cdcmd, DefaultHost, gen.DebugPort, waitarg, moduleAndArgs)

	return cdcmd + pycmd
}

func (gen *PythonStartupScriptGenerator) getAppPath() string {
	if gen.Manifest.CompressDestinationDir == "true" && gen.Manifest.SourceDirectoryInBuildContainer != "" {
		return gen.Manifest.SourceDirectoryInBuildContainer
	}

	return gen.AppPath
}

func appendArgs(currentArgs string, argToAppend string) string {
	if currentArgs != "" {
		currentArgs += " "
	}
	currentArgs += argToAppend
	return currentArgs
}

func (gen *PythonStartupScriptGenerator) getVirtualEnvPath(virtualEnvDir string, virtualEnvironmentName string) string {
	if gen.Manifest.CompressedVirtualEnvFile == "" || gen.SkipVirtualEnvExtraction {
		return virtualEnvDir
	}

	return "/" + virtualEnvironmentName
}

func (gen *PythonStartupScriptGenerator) getHandleVenvPresentInRootScript(virtualEnvDir string, virtualEnvironmentName string) string {
	scriptBuilder := strings.Builder{}

	scriptBuilder.WriteString("if [ -d " + virtualEnvironmentName + " ]; then\n")
	scriptBuilder.WriteString("    mv -f " + virtualEnvironmentName + " _del_" + virtualEnvironmentName + " || true\n")
	scriptBuilder.WriteString("fi\n\n")

	scriptBuilder.WriteString("if [ -d " + virtualEnvDir + " ]; then\n")
	scriptBuilder.WriteString("    ln -sfn " + virtualEnvDir + " ./" + virtualEnvironmentName + "\n")
	scriptBuilder.WriteString("fi\n\n")
	scriptBuilder.WriteString("echo \"Done.\"\n")
	
	return scriptBuilder.String()
}