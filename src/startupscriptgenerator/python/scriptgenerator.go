package main

import (
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

type PythonStartupScriptGenerator struct {
	SourcePath             string
	UserStartupCommand     string
	DefaultAppPath         string
	DefaultAppModule       string
	BindHost               string
	VirtualEnvironmentName string
	PackageDirectory       string
}

func (gen *PythonStartupScriptGenerator) GenerateEntrypointScript() string {
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	if gen.VirtualEnvironmentName != "" {
		scriptBuilder.WriteString(". " + gen.VirtualEnvironmentName + "/bin/activate\n")
		// TODO - gunicorn has to be installed in the virtual environenment for things to work correctly.
		// This will be one more benefit of getting rid of virtual envs, which is to be able to run gunicorn
		// from the image instead of from the virutal env.
		scriptBuilder.WriteString("\n# gunicorn has to be installed in the virtual environment\n")
		scriptBuilder.WriteString("pip install gunicorn\n")
	} else {
		packagedDir := filepath.Join(gen.SourcePath, gen.PackageDirectory)
		scriptBuilder.WriteString("# Check if the oryx packages folder is present, and if yes, add a .pth file for it so the interpreter can find it\n" +
			"ORYX_PACKAGES_PATH=" + packagedDir + "\n" +
			"if [ -d $ORYX_PACKAGES_PATH ]; then\n" +
			"  SITE_PACKAGES_PATH=$(python -c \"import site; print(site.getsitepackages()[0])\")\n" +
			"  echo $ORYX_PACKAGES_PATH > $SITE_PACKAGES_PATH\"/oryx.pth\"\n" +
			"  PATH=\"$ORYX_PACKAGES_PATH/bin:$PATH\"\n" +
			"fi\n")
	}
	scriptBuilder.WriteString("\n#Enter the source directory do make sure the  script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")
	command := gen.UserStartupCommand
	if command == "" {
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
	}
	scriptBuilder.WriteString(command + "\n")
	return scriptBuilder.String()
}

// Checks if the app is based on Django, and returns a startup command if so.
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
	filesToSearch := []string{"application.py", "app.py", "index.py", "server.py"}
	for _, file := range filesToSearch {
		fullPath := filepath.Join(gen.SourcePath, file)
		if _, err := os.Stat(fullPath); !os.IsNotExist(err) {
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
