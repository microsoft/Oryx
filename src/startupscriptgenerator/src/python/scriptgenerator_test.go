// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"os"
	"path/filepath"
	"testing"
	"github.com/stretchr/testify/assert"
)

func Test_ExamplePythonStartupScriptGenerator_buildGunicornCommandForModule_onlyModule(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		"\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "")

	// Assert
	assert.Equal(t, expected, actual)
}

func Test_buildGunicornCommandForModule_moduleAndPath(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		" --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}

// FastAPI: gunicorn command with uvicorn worker class
func Test_buildGunicornCommandForModule_FastAPI_withUvicornWorker(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		" --bind=0.0.0.0:80 --chdir=/home/site/wwwroot\" gunicorn " +
		"-k uvicorn_worker.UvicornWorker main:app"
	gen := PythonStartupScriptGenerator{
		BindPort: "80",
	}

	// Act
	actual := gen.buildGunicornCommandForModule(
		"-k uvicorn_worker.UvicornWorker main:app", "/home/site/wwwroot")

	// Assert
	assert.Equal(t, expected, actual)
}

// FastAPI: debug command with uvicorn
func Test_buildDebugPyCommandForModule_FastAPI(t *testing.T) {
	// Arrange
	// Note: cdcmd is prepended twice (once in Sprintf, once in return cdcmd + pycmd)
	expected := "cd /app && cd /app && python -m debugpy --listen 0.0.0.0:5678  -m " +
		"uvicorn main:app --host $HOST --port $PORT"
	gen := PythonStartupScriptGenerator{
		DebugPort: "5678",
	}

	// Act
	actual := gen.buildDebugPyCommandForModule(
		"uvicorn main:app --host $HOST --port $PORT", "/app")

	// Assert
	assert.Equal(t, expected, actual)
}

// FastAPI detection: main.py with FastAPI import, validates detection + all output methods
func Test_fastAPIDetector_detect_mainPy(t *testing.T) {
	// Arrange
	dir := t.TempDir()
	os.WriteFile(filepath.Join(dir, "main.py"),
		[]byte("from fastapi import FastAPI\napp = FastAPI()\n"), 0644)

	detector := &fastAPIDetector{appPath: dir}

	// Act & Assert
	assert.True(t, detector.detect())
	assert.Equal(t, "main.py", detector.mainFile)
	assert.Equal(t, "-k uvicorn_worker.UvicornWorker main:app", detector.GetGunicornModuleArg())
	assert.Equal(t, "uvicorn main:app --host $HOST --port $PORT", detector.GetDebuggableModule())
}

// FastAPI detection: no FastAPI import should return false
func Test_fastAPIDetector_detect_notFastAPI(t *testing.T) {
	// Arrange
	dir := t.TempDir()
	os.WriteFile(filepath.Join(dir, "main.py"),
		[]byte("from flask import Flask\napp = Flask(__name__)\n"), 0644)

	detector := &fastAPIDetector{appPath: dir}

	// Act & Assert
	assert.False(t, detector.detect())
}

// DetectFramework: FastAPI detected before Flask when app.py has FastAPI
func Test_DetectFramework_FastAPI_beforeFlask(t *testing.T) {
	// Arrange
	dir := t.TempDir()
	os.WriteFile(filepath.Join(dir, "app.py"),
		[]byte("from fastapi import FastAPI\napp = FastAPI()\n"), 0644)

	// Act
	fw := DetectFramework(dir, "")

	// Assert
	assert.NotNil(t, fw)
	assert.Equal(t, "FastAPI", fw.Name())
}

// DetectFramework: Django wins over FastAPI when wsgi.py exists in subdirectory
func Test_DetectFramework_Django_overFastAPI(t *testing.T) {
	// Arrange
	dir := t.TempDir()
	subDir := filepath.Join(dir, "myproject")
	os.MkdirAll(subDir, 0755)
	os.WriteFile(filepath.Join(subDir, "wsgi.py"), []byte(""), 0644)
	os.WriteFile(filepath.Join(dir, "main.py"),
		[]byte("from fastapi import FastAPI\napp = FastAPI()\n"), 0644)

	// Act
	fw := DetectFramework(dir, "")

	// Assert
	assert.NotNil(t, fw)
	assert.Equal(t, "Django", fw.Name())
}

func Test_buildGunicornCommandForModule_moduleAndPathAndHost(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		" --bind=0.0.0.0:12345 --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "12345",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}
