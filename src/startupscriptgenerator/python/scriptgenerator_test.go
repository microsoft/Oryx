package main

import (
	"fmt"
)

func ExamplePythonStartupScriptGenerator_getCommandFromModule_onlyModule() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindHost: "",
	}
	//Act
	command := gen.getCommandFromModule("module.py", "")
	fmt.Println(command)
	// Output:
	// gunicorn module.py
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPath() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindHost: "",
	}
	//Act
	command := gen.getCommandFromModule("module.py", "/a/b/c")
	fmt.Println(command)
	// Output:
	// GUNICORN_CMD_ARGS="--chdir=/a/b/c" gunicorn module.py
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPathAndHost() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindHost: "1.2.3.4",
	}
	//Act
	command := gen.getCommandFromModule("module.py", "/a/b/c")
	fmt.Println(command)
	// Output:
	// GUNICORN_CMD_ARGS="--bind=1.2.3.4 --chdir=/a/b/c" gunicorn module.py
}
