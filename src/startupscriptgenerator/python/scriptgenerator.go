package main

import (
	"io/ioutil"
	"path/filepath"
)

type PythonStartupScriptGenerator struct {
	SourcePath             string
	DefaultAppPath         string
	DefaultAppModule       string
	BindHost               string
	VirtualEnvironmentName string
}

func (gen *PythonStartupScriptGenerator) GenerateEntrypointCommand() string {
	command := ""
	appDirectory := gen.SourcePath
	appModule := gen.getDjangoStartupModule()
	if appModule == "" {
		appModule = gen.getFlaskStartupModule()
	}
	if appModule == "" {
		appDirectory = gen.DefaultAppPath
		appModule = gen.DefaultAppModule
	}

	if appModule != "" {
		command = gen.getCommandFromModule(appModule, appDirectory)
	}
	return command
}

// Checks if the app is based on Flask, and returns a startup command if so.
func (gen *PythonStartupScriptGenerator) getDjangoStartupModule() string {
	appRootFiles, err := ioutil.ReadDir(gen.SourcePath)
	if err != nil {
		panic("Couldn't read application folder '" + gen.SourcePath + "'")
	}
	for _, appRootFile := range appRootFiles {
		if appRootFile.IsDir() && appRootFile.Name() != gen.VirtualEnvironmentName {
			subDirPath := filepath.Join(gen.SourcePath, appRootFile.Name())
			subDirFiles, subDirErr := ioutil.ReadDir(subDirPath)
			if subDirErr != nil {
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
	appRootFiles, err := ioutil.ReadDir(gen.SourcePath)
	if err != nil {
		panic("Couldn't read application folder '" + gen.SourcePath + "'")
	}
	for _, appRootFile := range appRootFiles {
		if appRootFile.IsDir() == false && appRootFile.Name() == "application.py" {
			return "application:app"
		}
	}
	return ""
}

// Produces the gunicorn command to run the app
func (gen *PythonStartupScriptGenerator) getCommandFromModule(module string, appDir string) string {
	args := ""
	if gen.BindHost != "" {
		args += "--bind=" + gen.BindHost
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
