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

func NewDjangoDetector(appPath string, venvName string) PyAppFrameworkDetector {
	return djangoDetector{
		appPath: appPath,
		venvName: venvName,
	}
}

// Returns true if one of the subdirectories of the app has a file named 'wsgi.py'
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
