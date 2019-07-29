package main

import "common"

type PyAppFrameworkDetector interface {
    detect() bool
    getGunicornModuleArg() string
    getDebuggableCommand() string
}

type djangoDetector struct {
	appPath		string
	venvName	string
	wsgiFile	string
}

type flaskDetector struct {
	appPath		string
	mainFile	string
}

func NewDjangoDetector(appPath string, venvName string) PyAppFrameworkDetector {
	return djangoDetector{ appPath: appPath, venvName: venvName }
}

func NewFlaskDetector(appPath string) PyAppFrameworkDetector {
	return flaskDetector{ appPath: appPath }
}

// Checks if the app is based on Django:
// Returns true if one of the subdirectories of the app has a file named 'wsgi.py'.
func (detector *djangoDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.djangoDetector.detect")
	defer logger.Shutdown()

	appRootEntries, err := ioutil.ReadDir(detector.appPath)
	if err != nil {
		logReadDirError(logger, detector.appPath, err)
		panic("Couldn't read app directory '" + detector.appPath + "'")
	}

	for _, appRootEntry := range appRootEntries {
		if !appRootEntry.IsDir() || appRootEntry.Name() == detector.venvName {
			continue
		}

		subDirWsgiFilePath := filepath.Join(detector.appPath, appRootEntry.Name(), "wsgi.py")
		if common.FileExists(subDirWsgiFilePath) {
			detector.wsgiModule = appRootFile.Name() + ".wsgi"
			return true
		}
	}

	return false
}

func (detector *djangoDetector) getGunicornModuleArg() string {
	return detector.wsgiModule
}

func (detector *djangoDetector) getDebuggableCommand() string {
	if !common.FileExists(filepath.Join(detector.appPath, "manage.py")) {
		logger := common.GetLogger("python.frameworks.djangoDetector.getDebuggableCommand")
		logger.LogWarning("No 'manage.py' file found in app's root directory")
		logger.Shutdown()
	}

	return "manage.py startserver"
}

// Checks if the app is based on Flask:
// Returns true if there's a "application.py", "app.py", "index.py", or
// "server.py" file in the app's root.
func (detector *flaskDetector) detect() bool {
	logger := common.GetLogger("python.frameworks.flaskDetector.detect")
	defer logger.Shutdown()

	filesToSearch := []string{"application.py", "app.py", "index.py", "server.py"}

	for _, file := range filesToSearch {
		// TODO: app code might be under 'src'
		fullPath := filepath.Join(detector.appPath, file)
		if common.FileExists(fullPath) {
			logger.LogInformation("Found main file '%s'", fullPath)
			detector.mainFile = file
			return true
		}
	}

	return false
}

// TODO: detect correct variable name from a list of common names (app, application, etc.)
func (detector *flaskDetector) getGunicornModuleArg() string {
	module = detector.mainFile[0 : len(detector.mainFile) - 3] // Remove the '.py' from the end
	return module + ":app"
}

func (detector *flaskDetector) getDebuggableCommand() string {
	return detector.mainFile
}
