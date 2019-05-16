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
	scriptBuilder.WriteString("userStartUpCommand=\"" + gen.UserStartupCommand + "\"\n")
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
if [ ! -z "$userStartUpCommand" ]; then
	len=${#userStartUpCommand[@]}
	if [ "$len" -eq "1" ]; then
		if [ -f "${userStartUpCommand[$0]}" ]; then
		  startUpCommand="$userStartUpCommand"
		else
		  echo "Could not find the startup file '${userStartUpCommand[$0]}' on disk."
		fi
	elif [ "$len" -eq "2" ] && [ "${userStartUpCommand[$0]}" == "dotnet" ]; then
		if [ -f "${userStartUpCommand[$1]}" ]; then
		  startUpCommand="$userStartUpCommand"
		else
		  echo "Could not find the file '${userStartUpCommand[$1]}' on disk."
		fi
	else
		startUpCommand="$userStartUpCommand"
	fi
fi

if [ -z "$startUpCommand" ]; then
	echo Finding the startup file name...
	for file in *; do 
	if [ -f "$file" ]; then 
		case $file in
		*.runtimeconfig.json)
			startupDllFileNamePrefix=${file%%.runtimeconfig.json}
			startupExecutableFileName="$startupDllFileNamePrefix"
			startupDllFileName="$startupDllFileNamePrefix.dll"
			break
		;;
		esac
	fi
	done

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

	if [ -z "$startUpCommand" ] && [ -f "$startupDllFileName" ]; then
	  echo "Found the startup file '$startupDllFileName'"
	  startUpCommand="dotnet '$startupDllFileName'"
	fi
fi

if [ -z "$startUpCommand" ]; then
	if [ -f "$defaultAppFilePath" ]; then
	  cd "$defaultAppFileDir"
	  startUpCommand="dotnet '$defaultAppFilePath'"
	else
	  echo Unable to start the application.
	  exit 1
	fi
fi

echo "Running the command '$startUpCommand'..."
eval "$startUpCommand"
`
	scriptBuilder.WriteString(script)
	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}
