// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"encoding/xml"
	"io/ioutil"
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
	BindPort            string
}

type projectDetails struct {
	Name         string
	FullPath     string
	Directory    string
	CSProjectObj csProject
}

// Object models representing .NET Core '.csproj' file xml content
type csProject struct {
	XMLName    xml.Name        `xml:"Project"`
	Properties []propertyGroup `xml:"PropertyGroup"`
	ItemGroups []itemGroup     `xml:"ItemGroup"`
}

type propertyGroup struct {
	AssemblyName string `xml:"AssemblyName"`
}

type itemGroup struct {
	PackageReferences []packageReference `xml:"PackageReference"`
}

type packageReference struct {
	Name string `xml:"Include,attr"`
}

const ProjectEnvironmentVariableName = "PROJECT"
const OryxPublishOutputDirectory = "oryx_publish_output"
const DefaultBindPort = "8080"

var _retrievedProjectDetails = false
var _projDetails projectDetails = projectDetails{}

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation(
		"Generating script for source at '%s' and published output at '%s'",
		gen.SourcePath,
		gen.PublishedOutputPath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n\n")

	command, publishOutputDir := gen.getStartupCommand()

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

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
			logger.LogInformation("Default app file path was not provided. Could not generate a startup script.")
			return ""
		}
	}

	return scriptBuilder.String()
}

func (gen *DotnetCoreStartupScriptGenerator) getStartupCommand() (string, string) {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getStartupCommand")
	defer logger.Shutdown()

	// Get the publish output directory irrespective of whether the user supplied a custom startup command
	// as we want to generate a script which does a 'cd' to this directory and run the user startup command
	publishOutputDir := gen.PublishedOutputPath
	if publishOutputDir == "" {
		projDetails := gen.getProjectDetailsAndCache()
		if projDetails.FullPath == "" {
			return "", ""
		}

		publishOutputDir = filepath.Join(projDetails.Directory, OryxPublishOutputDirectory)

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
		projDetails := gen.getProjectDetailsAndCache()
		if projDetails.FullPath == "" {
			return "", ""
		}

		// Since an application's published output can contain many .dll files,
		// find the name of the .dll which has the entry point to the application.
		// Resolution logic:
		// If the .csproj file has an explicitly AssemblyName property element, then use that value to get the name of the .dll
		// else
		// Use the .csproj file name (excluding the .csproj extension name) as the name of the .dll

		assemblyName := getAssemblyNameFromProjectFile(projDetails)

		startupFileName := ""
		if assemblyName == "" {
			projectName := getFileNameWithoutExtension(projDetails.Name)
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
		logger.LogInformation("adding execution permission if needed ...");
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.SourcePath);
		logger.LogInformation("permission added %t", isPermissionAdded)
	}

	return command, publishOutputDir
}

func (gen *DotnetCoreStartupScriptGenerator) getProjectDetailsAndCache() projectDetails {
	if _retrievedProjectDetails {
		return _projDetails
	}

	projDetails := gen.getProjectDetails()
	_retrievedProjectDetails = true
	_projDetails = projDetails
	return _projDetails
}

func (gen *DotnetCoreStartupScriptGenerator) getProjectDetails() projectDetails {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getProjectDetails")
	defer logger.Shutdown()

	projDetails := projectDetails{}

	projectEnv := os.Getenv(ProjectEnvironmentVariableName)
	if projectEnv != "" {
		// Since relative paths are provided to the environment variable, get the full path
		projectFilePath := filepath.Join(gen.SourcePath, projectEnv)
		if _, err := os.Stat(projectFilePath); os.IsNotExist(err) {
			logger.LogError(
				"Could not find project file '%s'. Error: %s",
				projectFilePath,
				err.Error())
			return projDetails
		} else {
			projFileInfo, _ := os.Stat(projectFilePath)
			projDetails.Name = projFileInfo.Name()
			projDetails.FullPath = projectFilePath
			projDetails.Directory = filepath.Dir(projectFilePath)
			projDetails.CSProjectObj = deserializeProjectFile(projectFilePath)
			return projDetails
		}
	}

	rootProjectFile := gen.getRootProjectFile()
	if rootProjectFile != "" {
		projFileInfo, _ := os.Stat(rootProjectFile)
		projDetails.Name = projFileInfo.Name()
		projDetails.FullPath = rootProjectFile
		projDetails.Directory = filepath.Dir(rootProjectFile)
		projDetails.CSProjectObj = deserializeProjectFile(rootProjectFile)
		return projDetails
	}

	logger.LogInformation(
		"Could not find project file at directory '%s'. Searching sub-directories ...", gen.SourcePath)

	projectFiles, err := gen.findProjectFiles()
	if err != nil {
		logger.LogError(
			"An error occurred while trying to search the repository for a web application project: '%s'",
			err.Error())
		return projDetails
	}

	for _, projectFile := range projectFiles {
		csProjObj := deserializeProjectFile(projectFile)
		if csProjObj.ItemGroups == nil {
			continue
		}

		for _, itemGroup := range csProjObj.ItemGroups {
			if itemGroup.PackageReferences == nil {
				continue
			}

			for _, packageReference := range itemGroup.PackageReferences {
				if packageReference.Name == "" {
					continue
				}

				if packageReference.Name == "Microsoft.AspNetCore.App" ||
					packageReference.Name == "Microsoft.AspNetCore.All" ||
					packageReference.Name == "Microsoft.AspNetCore" {
					projFileInfo, _ := os.Stat(projectFile)
					projDetails.Name = projFileInfo.Name()
					projDetails.FullPath = projectFile
					projDetails.Directory = filepath.Dir(projectFile)
					projDetails.CSProjectObj = csProjObj
					break
				}
			}
		}
	}
	return projDetails
}

func (gen *DotnetCoreStartupScriptGenerator) getRootProjectFile() string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getProjgetRootProjectFilectFile")
	defer logger.Shutdown()

	repoFiles, err := ioutil.ReadDir(gen.SourcePath)
	if err != nil {
		logger.LogError(
			"Error occurred while trying to read the source directory '%s'. Error: %s",
			gen.SourcePath,
			err.Error())
		return ""
	}

	for _, file := range repoFiles {
		if file.Mode().IsRegular() {
			fileName := file.Name()
			if filepath.Ext(fileName) == ".csproj" {
				return filepath.Join(gen.SourcePath, fileName)
			}
		}
	}
	return ""
}

func getAssemblyNameFromProjectFile(projDetails projectDetails) string {
	// get the assembly name if defined /Project/PropertyGroup/AssemblyName
	csProjObj := projDetails.CSProjectObj
	assemblyName := ""
	for i := 0; i < len(csProjObj.Properties); i++ {
		assemblyName = csProjObj.Properties[i].AssemblyName
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

func (gen *DotnetCoreStartupScriptGenerator) findProjectFiles() ([]string, error) {
	projectFiles := []string{}
	err := filepath.Walk(gen.SourcePath, func(path string, f os.FileInfo, err error) error {
		if filepath.Ext(path) == ".csproj" {
			projectFiles = append(projectFiles, path)
		}
		return nil
	})
	return projectFiles, err
}

func deserializeProjectFile(projectFile string) csProject {
	logger := common.GetLogger("dotnetcore.scriptgenerator.deserializeProjectFile")

	projFile := csProject{}
	xmlFile, err := os.Open(projectFile)
	// if os.Open returns an error then handle it
	if err != nil {
		logger.LogError(
			"Error occurred when trying to read project file '%s'. Error: %s",
			projectFile,
			err.Error())
		return projFile
	}
	// defer the closing of our xmlFile so that we can parse it later on
	defer xmlFile.Close()

	byteValue, _ := ioutil.ReadAll(xmlFile)
	err = xml.Unmarshal(byteValue, &projFile)
	if err != nil {
		logger.LogError(
			"Error occurred when trying to deserialize project file '%s'. Error: %s",
			projectFile,
			err.Error())
	}
	return projFile
}
