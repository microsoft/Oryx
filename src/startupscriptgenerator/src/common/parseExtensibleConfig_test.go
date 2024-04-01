// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
    "path/filepath"
    "os"
    "testing"

    "common/consts"

    "github.com/stretchr/testify/assert"
    "github.com/google/uuid"
)

func Test_ParseExtensibleConfigFile_Succeeds_ForYamlWithSingleEnvValue(t *testing.T) {
    // Arrange
    content := 
`env:
  - name: FOO
    value: BAR`
    
    testDirectory := uuid.New().String()
    mkdirErr := os.Mkdir(testDirectory, os.ModePerm)
    if mkdirErr != nil {
        t.Errorf("Unable to create test directory")
    }

    defer os.RemoveAll(testDirectory)

    configFilePath := filepath.Join(testDirectory, consts.ExtensibleConfigurationFileName)
    configFile, createErr := os.Create(configFilePath)
    if createErr != nil {
        t.Errorf("Unable to create extensible configuration file")
    }

    _, writeErr := configFile.WriteString(content)
    if writeErr != nil {
        t.Errorf("Unable to write contents to extensible configuration file")
    }

    // Act
    commands := ParseExtensibleConfigFile(configFilePath)

    // Assert
    assert.NotEqual(t, "", commands)
    assert.Contains(t, commands, "export FOO='BAR'")
}

func Test_ParseExtensibleConfigFile_Succeeds_ForYamlWithMultipleEnvValue(t *testing.T) {
    // Arrange
    content := 
`env:
  - name: FOO
    value: BAR
  - name: HELLO
    value: WORLD`
    
    testDirectory := uuid.New().String()
    mkdirErr := os.Mkdir(testDirectory, os.ModePerm)
    if mkdirErr != nil {
        t.Errorf("Unable to create test directory")
    }

    defer os.RemoveAll(testDirectory)

    configFilePath := filepath.Join(testDirectory, consts.ExtensibleConfigurationFileName)
    configFile, createErr := os.Create(configFilePath)
    if createErr != nil {
        t.Errorf("Unable to create extensible configuration file")
    }

    _, writeErr := configFile.WriteString(content)
    if writeErr != nil {
        t.Errorf("Unable to write contents to extensible configuration file")
    }

    // Act
    commands := ParseExtensibleConfigFile(configFilePath)

    // Assert
    assert.NotEqual(t, "", commands)
    assert.Contains(t, commands, "export FOO='BAR'")
    assert.Contains(t, commands, "export HELLO='WORLD'")
    assert.NotContains(t, commands, "export FOO='BAR'export HELLO='WORLD'")
}

func Test_ParseExtensibleConfigFile_Succeeds_ForYamlWithNoEnvValue(t *testing.T) {
    // Arrange
    content := 
`base-os: debian
platform: nodejs
platform-version: 14`
    
    testDirectory := uuid.New().String()
    mkdirErr := os.Mkdir(testDirectory, os.ModePerm)
    if mkdirErr != nil {
        t.Errorf("Unable to create test directory")
    }

    defer os.RemoveAll(testDirectory)

    configFilePath := filepath.Join(testDirectory, consts.ExtensibleConfigurationFileName)
    configFile, createErr := os.Create(configFilePath)
    if createErr != nil {
        t.Errorf("Unable to create extensible configuration file")
    }

    _, writeErr := configFile.WriteString(content)
    if writeErr != nil {
        t.Errorf("Unable to write contents to extensible configuration file")
    }

    // Act
    commands := ParseExtensibleConfigFile(configFilePath)

    // Assert
    assert.Equal(t, "", commands)
}

func Test_ParseExtensibleConfigFile_Succeeds_ForNonexistentYaml(t *testing.T) {
    // Arrange    
    testDirectory := uuid.New().String()
    configFilePath := filepath.Join(testDirectory, consts.ExtensibleConfigurationFileName)

    // Act
    commands := ParseExtensibleConfigFile(configFilePath)

    // Assert
    assert.Equal(t, "", commands)
}

