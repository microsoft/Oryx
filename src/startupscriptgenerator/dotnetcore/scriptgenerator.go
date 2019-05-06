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
	SourcePath         string
	AppPath            string
	RunFromPath        string
	UserStartupCommand string
	DefaultAppFilePath string
	BindPort           string
	Manifest           common.BuildManifest
}

type projectDetails struct {
	Name         string
	FullPath     string
	Directory    string
	CSProjectObj csProject
}

// Object models representing .NET Core '.csproj' file xml content
type csProject struct {
	SdkName    string          `xml:"Project,Sdk"`
	Properties []propertyGroup `xml:"PropertyGroup"`
}

type propertyGroup struct {
	AssemblyName string `xml:"AssemblyName"`
}

const ProjectEnvironmentVariableName = "PROJECT"
const DefaultBindPort = "8080"

var _projDetails projectDetails = projectDetails{}

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *strings.Builder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation(
		"Generating script for source at '%s' and published output at '%s'",
		gen.SourcePath,
		gen.AppPath)

	startupDllFileName := gen.getStartupDllFileName()

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

	scriptBuilder.WriteString("readonly appPath=\"" + gen.RunFromPath + "\"\n")
	scriptBuilder.WriteString("userStartUpCommand=\"" + gen.UserStartupCommand + "\"\n")
	scriptBuilder.WriteString("startUpCommand=\"\"\n")
	scriptBuilder.WriteString("readonly startupDllFileName=\"" + startupDllFileName + "\"\n")
	scriptBuilder.WriteString("readonly defaultAppFilePath=\"" + gen.DefaultAppFilePath + "\"\n")
	scriptBuilder.WriteString("if [ ! -z \"$userStartUpCommand\" ]; then\n")
	scriptBuilder.WriteString("  cd \"$appPath\"\n")
	scriptBuilder.WriteString("  startUpCommand=\"$userStartUpCommand\"\n")
	scriptBuilder.WriteString("  if [ -f \"$userStartUpCommand\" ]; then\n")
	scriptBuilder.WriteString("    chmod 755 \"$userStartUpCommand\"\n")
	scriptBuilder.WriteString("  fi\n")
	scriptBuilder.WriteString("elif [ ! -z \"$startupDllFileName\" ]; then\n")
	scriptBuilder.WriteString("  cd \"$appPath\"\n")
	scriptBuilder.WriteString("  startUpCommand=\"dotnet $startupDllFileName\"\n")
	scriptBuilder.WriteString("elif [ ! -z \"$defaultAppFilePath\" ]; then\n")
	scriptBuilder.WriteString("  startUpCommand=\"dotnet $defaultAppFilePath\"\n")
	scriptBuilder.WriteString("else\n")
	scriptBuilder.WriteString("  echo Unable to start the application.\n")
	scriptBuilder.WriteString("  exit 1\n")
	scriptBuilder.WriteString("fi\n\n")
	scriptBuilder.WriteString("eval \"$startUpCommand\"\n\n")

	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}

func (gen *DotnetCoreStartupScriptGenerator) getStartupDllFileName() string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getStartupDllFileName")

	if gen.Manifest.StartupFileName != "" {
		logger.LogInformation(
			"Found startup file name as '%s' from build manifest file.",
			gen.Manifest.StartupFileName)
		return gen.Manifest.StartupFileName
	}

	projDetails := gen.getProjectDetails()
	if projDetails.FullPath == "" {
		logger.LogError("Could not find the project file.")
		return ""
	}

	// Since an application's published output can contain many .dll files,
	// find the name of the .dll which has the entry point to the application.
	// Resolution logic:
	// If the .csproj file has an explicitly AssemblyName property element,
	// then use that value to get the name of the .dll
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
	return startupFileName
}

func (gen *DotnetCoreStartupScriptGenerator) getProjectDetails() projectDetails {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getProjectDetails")
	defer logger.Shutdown()

	projDetails := projectDetails{}

	projectEnv := os.Getenv(ProjectEnvironmentVariableName)
	if projectEnv != "" {
		// Since relative paths are provided to the environment variable, get the full path
		projectFilePath := filepath.Join(gen.SourcePath, projectEnv)
		if !common.FileExists(projectFilePath) {
			logger.LogError("Could not find project file '%s'.", projectFilePath)
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

	// Filter for ASP.NET Core Web Application project files
	var webAppProjects []projectDetails
	for _, projectFile := range projectFiles {
		csProjObj := deserializeProjectFile(projectFile)
		if csProjObj.SdkName == "Microsoft.NET.Sdk.Web" {
			currProjDetails := projectDetails{}
			projFileInfo, _ := os.Stat(projectFile)
			currProjDetails.Name = projFileInfo.Name()
			currProjDetails.FullPath = projectFile
			currProjDetails.Directory = filepath.Dir(projectFile)
			currProjDetails.CSProjectObj = csProjObj

			webAppProjects = append(webAppProjects, currProjDetails)
		}
	}

	if len(webAppProjects) == 1 {
		return webAppProjects[0]
	} else if len(webAppProjects) > 1 {
		var webAppProjectFiles []string
		for _, webAppProject := range webAppProjects {
			webAppProjectFiles = append(webAppProjectFiles, webAppProject.FullPath)
		}
		printProjectFiles := strings.Join(webAppProjectFiles[:], ", ")
		logger.LogError(
			"Found multiple ASP.NET Core web application projects. Projects: '%s'",
			printProjectFiles)

		panic(
			"Found multiple ASP.NET Core web application projects. " +
			"Use the PROJECT environment variable to specify a repo relative path to the project file to consider.")
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
