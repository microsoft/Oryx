// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
    "fmt"
    "os"
    "strings"
    "gopkg.in/yaml.v3"
)

type EnvironmentVariable struct {
    Name  string `yaml:"name"`
    Value string `yaml:"value"`
}

type ExtensibleConfigFile struct {
    BaseOs               string                `yaml:"base-os"`
    Platform             string                `yaml:"platform"`
    PlatformVersion      string                `yaml:"platform-version"`
    EnvironmentVariables []EnvironmentVariable `yaml:"env"`
}

func ParseExtensibleConfigFile(sourcePath string) string {
    commands := ""
    fileContent, err := os.ReadFile(sourcePath)
    if err != nil {
        fmt.Println("Error when reading extensible config file")
        return ""
    }

    configFile := ExtensibleConfigFile{}
    yamlErr := yaml.Unmarshal(fileContent, &configFile)

    if yamlErr != nil {
        fmt.Println("Error when parsing extensible config file")
        return ""
    }

    if configFile.EnvironmentVariables != nil {
        for i := 0; i < len(configFile.EnvironmentVariables); i++ {
            name := strings.TrimSpace(configFile.EnvironmentVariables[i].Name)
            value := strings.TrimSpace(configFile.EnvironmentVariables[i].Value)
            commands += "export " + name + "='" + value + "'\n"
        }
    }

    return commands
}
