// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"io/ioutil"
	"path/filepath"
	"startupscriptgenerator/common"
	"startupscriptgenerator/common/consts"
	"strings"
	"strconv"
)

type PythonStartupScriptGenerator struct {
	SourcePath               string
	UserStartupCommand       string
	DefaultAppPath           string
	DefaultAppModule         string
	BindPort                 string
	VirtualEnvironmentName   string
	PackageDirectory         string
	SkipVirtualEnvExtraction bool
}

const DefaultHost = "0.0.0.0"
const DefaultBindPort = "80"

func (gen *PythonStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("python.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source at '%s'", gen.SourcePath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/bash\n")
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n\n")

	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	packageSetupBlock := gen.getPackageSetupCommand()
	scriptBuilder.WriteString(packageSetupBlock)

	appType := ""
	appModule := ""
	command := gen.UserStartupCommand
	if command == "" {
		appDirectory := gen.SourcePath
		appModule = gen.getDjangoStartupModule()

		if appModule == "" {
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
		} else {
			appType = "Django"
			println("Detected Django app.")
		}

		if appModule != "" {
			logger.LogInformation("Generating command for appModule='%s'", appModule)
			command = gen.getCommandFromModule(appModule, appDirectory)
		}
	} else {
		logger.LogInformation("adding execution permission if needed ...")
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.SourcePath)
		logger.LogInformation("permission added %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.SourcePath)
	}

	scriptBuilder.WriteString(command + "\n")

	logger.LogProperties("Finalizing script", map[string]string{"appType": appType, "appModule": appModule, "venv": gen.VirtualEnvironmentName})

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
	scriptBuilder.WriteString("sourcePath=\"" + gen.SourcePath + "\"\n")
	scriptBuilder.WriteString("skipVirtualEnvExtraction=\"" + strconv.FormatBool(gen.SkipVirtualEnvExtraction) + "\"\n")
	scriptBuilder.WriteString("virtualEnvName=\"" + gen.VirtualEnvironmentName + "\"\n")
	scriptBuilder.WriteString("packagedir=\"" + gen.PackageDirectory + "\"\n")
	scriptBuilder.WriteString("buildManifestFileName=\"" + consts.BuildManifestFileName + "\"\n")
	mainScript := `
if [ -f ./$buildManifestFileName ]; then
	echo "Found the build manifest file '$buildManifestFileName'. It's contents:"
	cat ./$buildManifestFileName
	echo
	echo "Sourcing the buid manifest file..."
	. ./$buildManifestFileName
else
	echo "Could not find the manifest file '$buildManifestFileName'."
fi

packagedir="$sourcePath/$packagedir"

if [ ! -z "$virtualEnvName" ]; then
	echo "Virtual environment '$virtualEnvName' is being used"
	echo "Checking if virtual environment was compressed..."
	if [ -z "$compressedVirtualEnvFile" ]; then
		echo "Virtual environment was not compressed."
	else
		echo "Virtual environment was compressed."
		if [ "$skipVirtualEnvExtraction" == "false" ]; then
			case $compressedVirtualEnvFile in
				*".zip")
					echo "Found zip-based virtual environment."
					extractionCommand="unzip -q $compressedVirtualEnvFile -d /$virtualEnvName"
					;;
				*".tar.gz")
					echo "Found tar.gz based virtual environment."
					extractionCommand="tar -xzf $compressedVirtualEnvFile -C /$virtualEnvName"
					;;
			esac
		else
			echo "Extracting compressed virtual environment directory as been disabled."
		fi
	fi

	if [ ! -z "$extractionCommand" ]; then
		echo "Removing existing virtual environment directory..."
		rm -fr /$virtualEnvName
		mkdir -p /$virtualEnvName
		echo "Extracting..."
		$extractionCommand
		virtualEnvDir="/$virtualEnvName"
	elif [ ! -z "$virtualEnvName" ]; then
		virtualEnvDir="$sourcePath/$virtualEnvName"
	fi

	echo "Virtual environment directory is set to '$virtualEnvDir'"

	# We add the virtual env site-packages to PYTHONPATH instead of activating it to be backwards compatible with existing
	# app service implementation. If we activate the virtual env directly things don't work since it has hardcoded references to
	# python libraries including the absolute path. Since Python is installed in different paths in build and runtime images,
	# the libraries are not found.
	PYTHON_VERSION=$(python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
	echo "Using packages from virtual environment '$virtualEnvName' located at '$virtualEnvDir'."
	virtualEnvSitePackagesDir="$virtualEnvDir/lib/python$PYTHON_VERSION/site-packages"
	export PYTHONPATH=$PYTHONPATH:$virtualEnvSitePackagesDir
	echo "Python path has been set to '$PYTHONPATH'"
elif [ -d "$packagedir" ]; then
	echo "Using package directory '$packagedir'"
	SITE_PACKAGE_PYTHON_VERSION=$(python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
	SITE_PACKAGES_PATH=$HOME"/.local/lib/python"$SITE_PACKAGE_PYTHON_VERSION"/site-packages"
	echo "Site packages path set to '$SITE_PACKAGES_PATH'"
	mkdir -p $SITE_PACKAGES_PATH
	echo "$packagedir" > $SITE_PACKAGES_PATH"/oryx.pth"
	PATH="$packagedir/bin:$PATH"
	echo "Updated PATH env variable: '$PATH'"
else
	# We just warn the user and don't error out, since we still can run the default website.
	echo "WARNING: Could not find packages folder or virtual environment."
fi
`
	scriptBuilder.WriteString(mainScript)
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
		if appRootFile.IsDir() && appRootFile.Name() != gen.VirtualEnvironmentName {
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
func (gen *PythonStartupScriptGenerator) getCommandFromModule(module string, appDir string) string {
	args := ""
	if gen.BindPort != "" {
		args += "--bind=" + DefaultHost + ":" + gen.BindPort
	}
	if appDir != "" {
		if args != "" {
			args += " "
		}
		args += "--chdir=" + appDir
	}
	if args != "" {
		return "GUNICORN_CMD_ARGS=\"" + args + "\" gunicorn " + module
	} else {
		return "gunicorn " + module
	}
}
