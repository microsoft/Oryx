package main

import (
	"encoding/xml"
	"io/ioutil"
	"log"
	"os"
	"path/filepath"
	"startupscriptgenerator/common"
	"strings"
)

type DotnetCoreStartupScriptGenerator struct {
	SourcePath          string
	PublishedOutputPath string
	UserStartupCommand  string
	DefaultAppFilePath  string
}

type project struct {
	XMLName    xml.Name        `xml:"Project"`
	Properties []propertyGroup `xml:"PropertyGroup"`
}

type propertyGroup struct {
	AssemblyName string `xml:"AssemblyName"`
}

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")

	logger.LogInformation(
		"Generating script for source at '%s' and published output at '%s'",
		gen.SourcePath,
		gen.PublishedOutputPath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n\n")

	command, publishOutputDir := gen.getStartupCommand()

	if command != "" {
		logger.LogInformation("Successfully generated startup command.")
		scriptBuilder.WriteString("cd \"" + publishOutputDir + "\"\n\n")
		scriptBuilder.WriteString(command + "\n\n")
	} else {
		if gen.DefaultAppFilePath != "" {
			logger.LogInformation(
				"Could not generate startup command. Using the default app file path to generate a command.")

			command = "dotnet \"" + gen.DefaultAppFilePath + "\""
			scriptBuilder.WriteString(command + "\n\n")
		} else {
			log.Fatal("Could not generate startup script.")
		}
	}
	return scriptBuilder.String()
}

func (gen *DotnetCoreStartupScriptGenerator) getStartupCommand() (string, string) {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getStartupCommand")

	publishOutputDir := gen.PublishedOutputPath
	if publishOutputDir == "" {
		publishOutputDir = filepath.Join(gen.SourcePath, "oryx_publish_output")

		logger.LogInformation(
			"Published output directory not supplied. Checking for default oryx publish output directory at '%s'",
			publishOutputDir)

		if _, err := os.Stat(publishOutputDir); os.IsNotExist(err) {
			logger.LogError(
				"Could not find oryx publish output directory at '%s'. Error: %s",
				publishOutputDir,
				err.Error())
			return "", ""
		} else {
			logger.LogInformation("Successfully found oryx publish output directory.")
		}
	}

	command := gen.UserStartupCommand
	if command == "" {
		// Since an application's published output can contain many .dll files,
		// find the name of the .dll which has the entry point to the application.
		// Resolution logic:
		// If the .csproj file has an explicitly AssemblyName property element, then use that value to get the name of the .dll
		// else
		// Use the .csproj file name (excluding the .csproj extension name) as the name of the .dll
		projectFile := getProjectFile(gen.SourcePath)

		if projectFile == nil {
			logger.LogError("Could not find project file in source directory '%s'", gen.SourcePath)
			return "", ""
		}

		assemblyName := getAssemblyNameFromProjectFile(gen.SourcePath, projectFile.Name())

		startupFileName := ""
		if assemblyName == "" {
			projectName := getFileNameWithoutExtension(projectFile.Name())
			startupFileName = projectName + ".dll"
		} else {
			startupFileName = assemblyName + ".dll"
		}

		// Check if the startup file is indeed present
		startupFileFullPath := filepath.Join(publishOutputDir, startupFileName)
		if _, err := os.Stat(startupFileFullPath); os.IsNotExist(err) {
			logger.LogError(
				"Could not find the startup file '%s'. Error: %s",
				startupFileFullPath,
				err.Error())
			return "", ""
		}

		command = "dotnet \"" + startupFileName + "\"\n"
	} else {
		logger.LogCritical("Using the explicit user provided startup command.")
	}

	return command, publishOutputDir
}

func getProjectFile(sourcePath string) os.FileInfo {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getProjectFile")

	repoFiles, err := ioutil.ReadDir(sourcePath)
	if err != nil {
		logger.LogError(
			"Error occurred while trying to read the source directory '%s'. Error: %s",
			sourcePath,
			err.Error())
		return nil
	}

	var projectFile os.FileInfo
	for _, file := range repoFiles {
		if file.Mode().IsRegular() {
			fileName := file.Name()
			if filepath.Ext(fileName) == ".csproj" {
				projectFile = file
				break
			}
		}
	}
	return projectFile
}

func getAssemblyNameFromProjectFile(sourcePath string, projectFileName string) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getAssemblyNameFromProjectFile")

	// get the assembly name if defined /Project/PropertyGroup/AssemblyName
	fullProjectFilePath := filepath.Join(sourcePath, projectFileName)
	xmlFile, err := os.Open(fullProjectFilePath)
	// if os.Open returns an error then handle it
	if err != nil {
		logger.LogError(
			"Error occurred when trying to read project file '%s'. Error: %s",
			fullProjectFilePath,
			err.Error())
		return ""
	}
	// defer the closing of our xmlFile so that we can parse it later on
	defer xmlFile.Close()

	byteValue, _ := ioutil.ReadAll(xmlFile)
	var projFile project
	err = xml.Unmarshal(byteValue, &projFile)
	if err != nil {
		logger.LogError(
			"Error occurred when trying to deserialize project file '%s'. Error: %s",
			fullProjectFilePath,
			err.Error())
		return ""
	}

	assemblyName := ""
	for i := 0; i < len(projFile.Properties); i++ {
		assemblyName = projFile.Properties[i].AssemblyName
		if assemblyName != "" {
			break
		}
	}
	return assemblyName
}

func getFileNameWithoutExtension(fileName string) string {
	index := strings.LastIndexByte(fileName, '.')
	if index >= 0 {
		return fileName[:index]
	}
	return fileName
}
