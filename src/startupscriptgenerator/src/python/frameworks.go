// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"fmt"
	"io/ioutil"
	"path/filepath"
	"strings"
)

type PyAppFramework interface {
    Name() string
    GetGunicornModuleArg() string
    GetDebuggableModule() string
    detect() bool
}

type djangoDetector struct {
	appPath		string
	venvName	string
	wsgiModule	string
}

type flaskDetector struct {
	appPath		string
	mainFile	string
}

type fastAPIDetector struct {
	appPath		string
	mainFile	string
}

func DetectFramework(appPath string, venvName string, pythonVersion string) PyAppFramework {
	var detector PyAppFramework

	detector = &djangoDetector{ appPath: appPath, venvName: venvName }
	if detector.detect() {
		return detector
	}

	if isPythonVersionAtLeast(pythonVersion, 3, 14) &&
		!common.GetBooleanEnvironmentVariable(consts.PythonDisableFastAPIDetectionEnvVarName) {
		detector = &fastAPIDetector{ appPath: appPath }
		if detector.detect() {
			return detector
		}
	}

	detector = &flaskDetector{ appPath: appPath }
	if detector.detect() {
		return detector
	}

	return nil
}

// isPythonVersionAtLeast checks whether the given version string (e.g. "3.14.1")
// is at least major.minor.
func isPythonVersionAtLeast(version string, major int, minor int) bool {
	parts := strings.SplitN(version, ".", 3)
	if len(parts) < 2 {
		return false
	}
	var maj, min int
	if _, err := fmt.Sscan(parts[0], &maj); err != nil {
		return false
	}
	if _, err := fmt.Sscan(parts[1], &min); err != nil {
		return false
	}
	return maj > major || (maj == major && min >= minor)
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
	module := detector.mainFile[0 : len(detector.mainFile) - 3] // Remove the '.py' from the end
	return module + ":app"
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

func (detector *fastAPIDetector) Name() string {
	return "FastAPI"
}

// Checks if the app is based on FastAPI:
// Returns true if a common entrypoint file both imports the fastapi package
// AND creates a FastAPI application instance.
func (detector *fastAPIDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.fastAPIDetector.detect")
	defer logger.Shutdown()

	filesToSearch := []string{"main.py", "app.py", "application.py", "server.py", "asgi.py", "api.py", "index.py", "run.py"}

	for _, file := range filesToSearch {
		fullPath := filepath.Join(detector.appPath, file)
		if !common.FileExists(fullPath) {
			continue
		}

		content, err := ioutil.ReadFile(fullPath)
		if err != nil {
			logger.LogError("Failed to read file '%s': %s", fullPath, err.Error())
			continue
		}

		contentStr := string(content)

		hasImport := strings.Contains(contentStr, "from fastapi") ||
			strings.Contains(contentStr, "import fastapi")
		hasFlask := strings.Contains(contentStr, "from flask")

		if hasImport && !hasFlask {
			logger.LogInformation("Detected FastAPI app in '%s'.", file)
			detector.mainFile = file
			return true
		}
	}

	return false
}

// Returns the gunicorn module argument with uvicorn worker class for ASGI support.
func (detector *fastAPIDetector) GetGunicornModuleArg() string {
	module := detector.mainFile[0 : len(detector.mainFile) - 3] // Remove the '.py' from the end
	return "-k uvicorn_worker.UvicornWorker " + module + ":app"
}

func (detector *fastAPIDetector) GetDebuggableModule() string {
	module := detector.mainFile[0 : len(detector.mainFile) - 3] // Remove the '.py' from the end
	// Use uvicorn directly for debugging since it supports --reload
	return fmt.Sprintf("uvicorn %s:app --host $HOST --port $PORT", module)
}
