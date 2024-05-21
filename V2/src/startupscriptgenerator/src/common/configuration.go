// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"fmt"
	"path"
	"strings"

	"github.com/spf13/viper"
)

// Order of precedence for value of settings: Explicit command like argument => Environment Variable
func SetEnvironmentVariableInScript(scriptBuilder *strings.Builder, environmentVariableName string, explicitValue string, defaultValue string) {
	if explicitValue != "" {
		scriptBuilder.WriteString("export " + environmentVariableName + "=" + explicitValue)
	} else {
		scriptBuilder.WriteString("if [ -z \"$" + environmentVariableName + "\" ]; then" + "\n")
		scriptBuilder.WriteString("		export " + environmentVariableName + "=" + defaultValue + "\n")
		scriptBuilder.WriteString("fi")
	}
	scriptBuilder.WriteString("\n\n")
}

func GetViperConfiguration(appPath string) *viper.Viper {
	configFileName := "build.env"

	viperConfig := viper.New()

	configFileFullPath := path.Join(appPath, configFileName)
	if FileExists(configFileFullPath) {
		viperConfig.SetConfigFile(configFileFullPath)
		viperConfig.AddConfigPath(appPath)

		err := viperConfig.ReadInConfig()
		if err != nil {
			panic(fmt.Sprintf(
				"Error reading configuration file '%s' at '%s'. \nError: %s",
				configFileName,
				appPath,
				err))
		}
	}

	// Enable VIPER to read Environment Variables
	viperConfig.AutomaticEnv()

	return viperConfig
}
