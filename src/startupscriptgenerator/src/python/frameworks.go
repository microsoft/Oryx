// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"bufio"
	"common"
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

type PyAppFramework interface {
	Name() string
	GetGunicornModuleArg() string
	GetUvicornModuleArg() string
	GetDebuggableModule() string
	detect() bool
}

type djangoDetector struct {
	appPath    string
	venvName   string
	wsgiModule string
}

type flaskDetector struct {
	appPath  string
	mainFile string
}

type fastApiDetector struct {
	appPath    string
	mainFile   string
	launchPath string
}

func DetectFramework(appPath string, venvName string) PyAppFramework {
	var detector PyAppFramework

	detector = &fastApiDetector{appPath: appPath}
	if detector.detect() {
		return detector
	}

	detector = &djangoDetector{appPath: appPath, venvName: venvName}
	if detector.detect() {
		return detector
	}

	detector = &flaskDetector{appPath: appPath}
	if detector.detect() {
		return detector
	}

	return nil
}

func (detector *fastApiDetector) Name() string {
	return "FastAPI"
}

// Checks if the app is based on FastAPI
func (detector *fastApiDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.fastApiDetector.detect")
	defer logger.Shutdown()

	filesToSearch := []string{"app.py", "main.py"}

	for _, file := range filesToSearch {
		fullPath := filepath.Join(detector.appPath, file)
		if common.FileExists(fullPath) {
			// Match on something that looks like the following in the main file:
			//	app = FastAPI()
			//	app = fastapi.FastAPI()
			//	app = FastAPI(
			f, err := os.Open(fullPath)
			if err != nil {
				logger.LogWarning("Failed to open and read file %s for FastAPI detection: %s", fullPath, err.Error())
				continue
			}

			defer f.Close()

			scanner := bufio.NewScanner(f)
			for scanner.Scan() {
				if strings.Contains(scanner.Text(), "FastAPI(") {
					decl := strings.Split(scanner.Text(), " ")
					if len(decl) >= 3 && decl[1] == "=" {
						detector.mainFile = fullPath
						mainObjName := decl[0]
						relativePath, err := filepath.Rel(detector.appPath, fullPath)
						if err != nil {
							continue
						}

						// dir1/dir2/main.py -> dir1.dir2.main
						mainPath := strings.ReplaceAll(strings.TrimSuffix(relativePath, ".py"), "/", ".")

						detector.launchPath = fmt.Sprintf("%s:%s", mainPath, mainObjName)
						return true
					}
				}
			}
		}
	}

	return false
}

func (detector *fastApiDetector) GetGunicornModuleArg() string {
	return ""
}

func (detector *fastApiDetector) GetUvicornModuleArg() string {
	return detector.launchPath
}

func (detector *fastApiDetector) GetDebuggableModule() string {
	return ""
}

func (detector *djangoDetector) Name() string {
	return "Django"
}

// Checks if the app is based on Django:
// Returns true if one of the subdirectories of the app has a file named 'wsgi.py'.
func (detector *djangoDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.djangoDetector.detect")
	defer logger.Shutdown()

	appRootEntries, err := ioutil.ReadDir(detector.appPath)
	if err != nil {
		logger.LogError("ioutil.ReadDir() failed: %s", err.Error())
		panic("Couldn't read app directory '" + detector.appPath + "'")
	}

	for _, appRootEntry := range appRootEntries {
		if !appRootEntry.IsDir() || appRootEntry.Name() == detector.venvName {
			continue
		}

		subDirWsgiFilePath := filepath.Join(detector.appPath, appRootEntry.Name(), "wsgi.py")
		if common.FileExists(subDirWsgiFilePath) {
			detector.wsgiModule = appRootEntry.Name() + ".wsgi"
			return true
		}
	}

	return false
}

func (detector *djangoDetector) GetGunicornModuleArg() string {
	return detector.wsgiModule
}

func (detector *djangoDetector) GetUvicornModuleArg() string {
	return ""
}

func (detector *djangoDetector) GetDebuggableModule() string {
	if !common.FileExists(filepath.Join(detector.appPath, "manage.py")) {
		logger := common.GetLogger("python.frameworks.djangoDetector.GetDebuggableModule")
		logger.LogWarning("No 'manage.py' file found in app's root directory")
		logger.Shutdown()
	}

	// Default is 127.0.0.1:8000 (https://docs.djangoproject.com/en/2.2/ref/django-admin/#runserver)
	return "manage runserver --noreload --nothreading $HOST:$PORT"
}

func (detector *flaskDetector) Name() string {
	return "Flask"
}

// Checks if the app is based on Flask:
// Returns true if there's a "application.py", "app.py", "index.py", or
// "server.py" file in the app's root.
func (detector *flaskDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.flaskDetector.detect")
	defer logger.Shutdown()

	// Warning: the official "flask run" tool only looks for "wsgi.py" or "app.py".
	// If the user is trying to debug an app with a differene main module name, it will need a custom debug command.
	filesToSearch := []string{"application.py", "app.py", "run.py", "index.py", "server.py", "wsgi.py"}

	for _, file := range filesToSearch {
		// TODO: app code might be under 'src'
		fullPath := filepath.Join(detector.appPath, file)
		if common.FileExists(fullPath) {
			logger.LogInformation("Found main file.")
			detector.mainFile = file
			return true
		}
	}

	return false
}

// TODO: detect correct variable name from a list of common names (app, application, etc.)
func (detector *flaskDetector) GetGunicornModuleArg() string {
	module := detector.mainFile[0 : len(detector.mainFile)-3] // Remove the '.py' from the end
	return module + ":app"
}

func (detector *flaskDetector) GetUvicornModuleArg() string {
	return ""
}

func (detector *flaskDetector) GetDebuggableModule() string {
	if !common.FileExists(filepath.Join(detector.appPath, "wsgi.py")) &&
		!common.FileExists(filepath.Join(detector.appPath, "app.py")) {
		logger := common.GetLogger("python.frameworks.flaskDetector.GetDebuggableModule")
		logger.LogWarning("No 'wsgi.py' or 'app.py' file found in app's root directory")
		logger.Shutdown()
	}

	// Default is 127.0.0.1:5000 (https://flask.palletsprojects.com/en/1.1.x/api/#flask.Flask.run)
	return fmt.Sprintf("flask run --host $HOST --port $PORT")
}
