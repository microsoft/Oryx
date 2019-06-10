// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"path/filepath"
	"startupscriptgenerator/common"
	"strings"
)

type DotnetCoreStartupScriptGenerator struct {
	AppPath            string
	RunFromPath        string
	UserStartupCommand string
	DefaultAppFilePath string
	BindPort           string
	Manifest           common.BuildManifest
}

const DefaultBindPort = "8080"
const RuntimeConfigJsonExtension = "runtimeconfig.json"

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *strings.Builder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for published output at '%s'", gen.AppPath)

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

	defaultAppFileDir := filepath.Dir(gen.DefaultAppFilePath)

	scriptBuilder.WriteString("readonly appPath=\"" + gen.RunFromPath + "\"\n")
	scriptBuilder.WriteString("userStartUpCommand=(" + gen.UserStartupCommand + ")\n")
	scriptBuilder.WriteString("startUpCommand=\"\"\n")
	scriptBuilder.WriteString("readonly defaultAppFileDir=\"" + defaultAppFileDir + "\"\n")
	scriptBuilder.WriteString("readonly defaultAppFilePath=\"" + gen.DefaultAppFilePath + "\"\n")

	script := `
isLinuxExecutable() {
	local file="$1"
	if [ -x "$file" ] && file "$file" | grep -q "GNU/Linux"
	then
	  isLinuxExecutableResult="true"
	else
	  isLinuxExecutableResult="false"
	fi
}

cd "$appPath"
len=${#userStartUpCommand[@]}
startupCommandString="${userStartUpCommand[@]}"
if [ $len -ne 0 ]; then
	echo "Trying to use provided startup command: $startupCommandString"
	isValid=true
	if [ $len -eq 1 ]; then
		file="${userStartUpCommand[0]}"
		if [ ! -f "$file" ]; then
			isValid=false
		  	echo "WARNING: Could not find the startup file '$file' on disk."
		fi
	elif [ $len -eq 2 ] && [ "${userStartUpCommand[0]}" == "dotnet" ]; then
		if [ ! -f "${userStartUpCommand[1]}" ]; then
			isValid=false
			echo "WARNING: Could not find the file '${userStartUpCommand[1]}' on disk."
		fi
	fi

	if [ $isValid = true ]; then
		startUpCommand="$startupCommandString"
	fi
else
	echo "Startup command was not provided, finding the startup file name..."
	runtimeConfigJsonFiles=()
	for file in *; do
		if [ -f "$file" ]; then
			case $file in
				*.runtimeconfig.json)
					runtimeConfigJsonFiles+=("$file")
				;;
			esac
		fi
	done

	fileCount=${#runtimeConfigJsonFiles[@]}
	if [ $fileCount -eq 1 ]; then
		file=${runtimeConfigJsonFiles[0]}
		startupDllFileNamePrefix=${file%%.runtimeconfig.json}
		startupExecutableFileName="$startupDllFileNamePrefix"
		startupDllFileName="$startupDllFileNamePrefix.dll"
	else
		echo "WARNING: Unable to find the startup dll file name."
		echo "WARNING: Expected to find only one file with extension 'runtimeconfig.json' but found $fileCount"

		if [ $fileCount -gt 1 ]; then
			echo "WARNING: Found files: ${runtimeConfigJsonFiles[@]}"
			echo "WARNING: To fix this issue you can set the startup command to point to a particular startup file"
			echo "         For example: 'dotnet myapp.dll'"
		fi
	fi

	if [ -f "$startupExecutableFileName" ]; then
		# Starting ASP.NET Core 3.0, an executable is created based on the platform where it is published from,
		# so for example, if a user does a publish (not self-contained) on Mac, there would be files like 'todoApp'
		# and 'todoApp.dll'. In this scenario the 'todoApp' executable is actually meant for Mac and not for Linux.
		# So here we check for the file type and fall back to using 'dotnet todoApp.dll'.

		isLinuxExecutable $startupExecutableFileName
		if [ "$isLinuxExecutableResult" == "true" ]; then
			echo "Found the startup executable file '$startupExecutableFileName'"
			startUpCommand="./$startupExecutableFileName"
		else
			echo "Cannot use executable '$startupExecutableFileName' as startup file as it is not meant for Linux"
		fi
	fi

	if [ -z "$startUpCommand" ] && [ ! -z "$startupDllFileName" ]; then
		if [ -f "$startupDllFileName" ]; then
			echo "Found the startup file '$startupDllFileName'"
			startUpCommand="dotnet '$startupDllFileName'"
		else
			echo "Cound not find the startup dll file '$startupDllFileName'"
		fi
	fi
fi

if [ -z "$startUpCommand" ]; then
	echo "Trying to run the default app instead..."
	if [ ! -z "$defaultAppFilePath" ]; then
		if [ -f "$defaultAppFilePath" ]; then
			cd "$defaultAppFileDir"
			startUpCommand="dotnet '$defaultAppFilePath'"
		else
			echo "Could not find the default app file '$defaultAppFilePath'"
		fi
	else
		echo "Default app was not provided. Unable to start the application."
	fi
fi

if [ -z "$startUpCommand" ]; then
	exit 1
fi

echo "Running the command '$startUpCommand'..."
eval "$startUpCommand"
`
	scriptBuilder.WriteString(script)
	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}
