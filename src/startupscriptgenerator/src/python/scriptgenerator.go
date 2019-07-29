// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	"runtime"
	"strconv"
	"strings"
)


type PythonStartupScriptGenerator struct {
	SourcePath					string
	UserStartupCommand			string
	DefaultAppPath				string
	DefaultAppModule			string
	DebugAdapter				string // Remote debugger to use.
									   //  Currently, only `ptvsd` is supported.
	DebugWait					bool   // Whether debugger should pause and wait for a client
									   //  connection before running the app
	BindPort					string
	VirtualEnvironmentName		string
	PackageDirectory			string
	SkipVirtualEnvExtraction	bool
	Manifest					common.BuildManifest
}

const SupportedDebugAdapter = "ptvsd"; // Not using an array since there's only one at the moment
const DefaultPtvsdPort = "3000"

const DefaultHost = "0.0.0.0"
const DefaultBindPort = "80"

func (gen *PythonStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("python.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source at '%s'", gen.SourcePath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n\n")

	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)

	packageSetupBlock := gen.getPackageSetupCommand()
	scriptBuilder.WriteString(packageSetupBlock)

	appDebugAdapter := "" // Will the app be started in debugging mode
	appType := ""
	appModule := ""
	command := gen.UserStartupCommand
	if command == "" {
		appDirectory := gen.SourcePath
		
		appModule = gen.getDjangoStartupModule()
		if appModule != "" {
			appType = "Django"
			println("Detected Django app.")
		} else {
			appModule = gen.getFlaskStartupModule()
			if appModule == "" {
				appType = "Default"
				logger.LogInformation("Using default app '%s'", gen.DefaultAppPath)
				println("Using default app from " + gen.DefaultAppPath)
				appDirectory = gen.DefaultAppPath
				appModule = gen.DefaultAppModule
			} else {
				appType = "Flask"
				println("Detected flask app.")
			}
		}

		if appModule != "" {
			if gen.shouldStartAppInDebugMode() {
				logger.LogInformation("Generating debug command for appModule='%s'", appModule)
				command = gen.buildPtvsdCommandForModule(appModule, appDirectory)
				appDebugAdapter = gen.DebugAdapter
			} else {
				logger.LogInformation("Generating command for appModule='%s'", appModule)
				command = gen.buildGunicornCommandForModule(appModule, appDirectory)
			}
		}
	} else {
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.SourcePath)
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.SourcePath)
	}

	scriptBuilder.WriteString(command + "\n")

	logger.LogProperties(
		"Finalizing script",
		map[string]string { "appType": appType, "appDebugAdapter": appDebugAdapter,
							"appModule": appModule, "venv": gen.Manifest.VirtualEnvName })

	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}

func logReadDirError(logger *common.Logger, path string, err error) {
	logger.LogError("ioutil.ReadDir('%s') failed: %s", path, err.Error())
}

// Builds the commands to setup the Python packages, using virtual env or a package folder.
func (gen *PythonStartupScriptGenerator) getPackageSetupCommand() string {
	scriptBuilder := strings.Builder{}

	// Values in manifest file takes precedence over values supplied at command line
	virtualEnvironmentName := gen.Manifest.VirtualEnvName
	if virtualEnvironmentName == "" {
		virtualEnvironmentName = gen.VirtualEnvironmentName
	}

	packageDirName := gen.Manifest.PackageDir
	if packageDirName == "" {
		packageDirName = gen.PackageDirectory
	}

	if virtualEnvironmentName != "" {
		virtualEnvDir := filepath.Join(gen.SourcePath, virtualEnvironmentName)

		// If virtual environment was not compressed or if it is compressed but mounted using a zip driver,
		// we do not want to extract the compressed file
		if gen.Manifest.CompressedVirtualEnvFile == "" || gen.SkipVirtualEnvExtraction {
			if common.PathExists(virtualEnvDir) {
				// We add the virtual env site-packages to PYTHONPATH instead of activating it to be backwards compatible with existing
				// app service implementation. If we activate the virtual env directly things don't work since it has hardcoded references to
				// python libraries including the absolute path. Since Python is installed in different paths in build and runtime images,
				// the libraries are not found.
				virtualEnvCommand := getVirtualEnvironmentCommand(virtualEnvironmentName, virtualEnvDir)
				scriptBuilder.WriteString(virtualEnvCommand)

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
			virtualEnvCommand := getVirtualEnvironmentCommand(virtualEnvironmentName, virtualEnvDir)
			scriptBuilder.WriteString(virtualEnvCommand)
		}
	}

	if packageDirName != "" {
		packageDir := filepath.Join(gen.SourcePath, packageDirName)
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

func getVirtualEnvironmentCommand(virtualEnvName string, virtualEnvDir string) string {
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

// Checks if the app is based on Django, and returns a startup command if so.
func (gen *PythonStartupScriptGenerator) getDjangoStartupModule() string {
	logger := common.GetLogger("python.scriptgenerator.getDjangoStartupModule")
	defer logger.Shutdown()

	appRootFiles, err := ioutil.ReadDir(gen.SourcePath)
	if err != nil {
		logReadDirError(logger, gen.SourcePath, err)
		panic("Couldn't read application folder '" + gen.SourcePath + "'")
	}
	for _, appRootFile := range appRootFiles {
		if appRootFile.IsDir() && appRootFile.Name() != gen.Manifest.VirtualEnvName {
			subDirPath := filepath.Join(gen.SourcePath, appRootFile.Name())
			subDirFiles, subDirErr := ioutil.ReadDir(subDirPath)
			if subDirErr != nil {
				logReadDirError(logger, subDirPath, subDirErr)
				panic("Couldn't read directory '" + subDirPath + "'")
			}
			for _, subDirFile := range subDirFiles {
				if subDirFile.IsDir() == false && subDirFile.Name() == "wsgi.py" {
					return appRootFile.Name() + ".wsgi"
				}
			}
		}
	}
	return ""
}

// Checks if the app is based on Flask, and returns a startup command if so.
func (gen *PythonStartupScriptGenerator) getFlaskStartupModule() string {
	logger := common.GetLogger("python.scriptgenerator.getFlaskStartupModule")
	defer logger.Shutdown()

	filesToSearch := []string{"application.py", "app.py", "index.py", "server.py"}

	for _, file := range filesToSearch {
		fullPath := filepath.Join(gen.SourcePath, file)
		if common.FileExists(fullPath) {
			logger.LogInformation("Found file '%s'", fullPath)
			println("Found file '" + fullPath + "' to run the app with.")
			// Remove the '.py' from the end to get the module name
			modulename := file[0 : len(file)-3]
			return modulename + ":app"
		}
	}

	return ""
}

// Produces the gunicorn command to run the app
func (gen *PythonStartupScriptGenerator) buildGunicornCommandForModule(module string, appDir string) string {
	workerCount := getWorkerCount()

	// Default to AppService's timeout value (in seconds)
	args := "--timeout 600 --access-logfile '-' --error-logfile '-' --workers=" + workerCount

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

func (gen *PythonStartupScriptGenerator) buildPtvsdCommandForModule(module string, appDir string) string {
	waitarg := ""
	if gen.DebugWait {
		waitarg = " --wait"
	}

	pycmd := "python -m ptvsd --host " + DefaultHost + " --port " + DefaultPtvsdPort + waitarg + " -m " + module

	cdcmd := ""
	if appDir != "" {
		cdcmd = "cd " + appDir + " && "
	}

	return cdcmd + pycmd
}

func appendArgs(currentArgs string, argToAppend string) string {
	if currentArgs != "" {
		currentArgs += " "
	}
	currentArgs += argToAppend
	return currentArgs
}

func getWorkerCount() string {
	// http://docs.gunicorn.org/en/stable/design.html#how-many-workers
	cpuCount := runtime.NumCPU()
	workerCount := (2 * cpuCount) + 1
	return strconv.Itoa(workerCount)
}
