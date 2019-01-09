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
	scriptBuilder.WriteString("\n#Enter the source directory do make sure the  script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")

	packagedDir := filepath.Join(gen.SourcePath, gen.PackageDirectory)
	scriptBuilder.WriteString("# Check if the oryx packages folder is present, and if yes, add a .pth file for it so the interpreter can find it\n" +
		"ORYX_PACKAGES_PATH=" + packagedDir + "\n" +
		"if [ -d $ORYX_PACKAGES_PATH ]; then\n" +
		"  SITE_PACKAGE_PYTHON_VERSION=$(python -c \"import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))\")\n" +
		"  SITE_PACKAGES_PATH=$HOME\"/.local/lib/python\"$SITE_PACKAGE_PYTHON_VERSION\"/site-packages\"\n" +
		"  mkdir -p $SITE_PACKAGES_PATH\n" +
		"  echo $ORYX_PACKAGES_PATH > $SITE_PACKAGES_PATH\"/oryx.pth\"\n" +
		"  PATH=\"$ORYX_PACKAGES_PATH/bin:$PATH\"\n")

	// If the build was created with an earlier version of the build image that created virtual environments,
	// we still use it for backwards compatibility.
	if gen.VirtualEnvironmentName != "" {
		scriptBuilder.WriteString("elif [ -d " + gen.VirtualEnvironmentName + " ]; then\n")
		scriptBuilder.WriteString("  echo \"Using virtual environment " + gen.VirtualEnvironmentName + ".\"\n")
		scriptBuilder.WriteString("  . " + gen.VirtualEnvironmentName + "/bin/activate\n")
	}

	scriptBuilder.WriteString("else\n")
	// We just warn the user and don't error out, since we still can run the default website.
	scriptBuilder.WriteString("  echo \"WARNING: Could not find packages folder or virtual environment.\"\n")
	scriptBuilder.WriteString("fi\n")
	command := gen.UserStartupCommand
	if command == "" {
		appDirectory := gen.SourcePath
		appModule := gen.getDjangoStartupModule()
		if appModule == "" {
			appModule = gen.getFlaskStartupModule()
			if appModule == "" {
				println("Using default app from " + gen.DefaultAppPath)
				appDirectory = gen.DefaultAppPath
				appModule = gen.DefaultAppModule
			} else {
				println("Detected flask app.")
			}
		} else {
			println("Detected Django app.")
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
			println("Found file '" + fullPath + "' to run the app with.")
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
