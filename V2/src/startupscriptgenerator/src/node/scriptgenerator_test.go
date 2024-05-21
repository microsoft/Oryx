// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
	"testing"

	"github.com/stretchr/testify/assert"
)

func ExampleNodeStartupScriptGenerator_getPackageJsonStartCommand_subDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	packageJson := &packageJson{
		Scripts: &packageJsonScripts{
			Start: "node server.js",
		},
	}
	command := gen.getPackageJsonStartCommand(packageJson, "/a/b/c/package.json")
	fmt.Println(command)
	// Output:
	// npm --prefix=/a/b/c start
}

func ExampleNodeStartupScriptGenerator_getPackageJsonStartCommand_rootDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	packageJson := &packageJson{
		Scripts: &packageJsonScripts{
			Start: "node server.js",
		},
	}
	command := gen.getPackageJsonStartCommand(packageJson, "/a/b/package.json")
	fmt.Println(command)
	// Output:
	// npm start
}

func ExampleNodeStartupScriptGenerator_getPackageJsonStartCommand_subDirDebug() {
	gen := &NodeStartupScriptGenerator{
		SourcePath:      "/a/b",
		RemoteDebugging: true,
	}
	packageJson := &packageJson{
		Scripts: &packageJsonScripts{
			Start: "node server.js",
		},
	}
	command := gen.getPackageJsonStartCommand(packageJson, "/a/b/c/package.json")
	fmt.Println(command)
	// Output:
	// export PATH=/opt/node-wrapper/:$PATH
	// export ORYX_NODE_INSPECT_PARAM="--inspect=0.0.0.0"
	// npm --prefix=/a/b/c start --scripts-prepend-node-path false
}

func ExampleNodeStartupScriptGenerator_getPackageJsonStartCommand_rootDirDebug() {
	gen := &NodeStartupScriptGenerator{
		SourcePath:      "/a/b",
		RemoteDebugging: true,
	}
	packageJson := &packageJson{
		Scripts: &packageJsonScripts{
			Start: "node server.js",
		},
	}
	command := gen.getPackageJsonStartCommand(packageJson, "/a/b/package.json")
	fmt.Println(command)
	// Output:
	// export PATH=/opt/node-wrapper/:$PATH
	// export ORYX_NODE_INSPECT_PARAM="--inspect=0.0.0.0"
	// npm start --scripts-prepend-node-path false
}

func ExampleNodeStartupScriptGenerator_getUserProvidedJsFileCommand_sameDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	command := gen.getUserProvidedJsFileCommand("/a/b/server.js")
	fmt.Println(command)
	// Output:
	// node server.js
}

func ExampleNodeStartupScriptGenerator_getUserProvidedJsFileCommand_subDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	command := gen.getUserProvidedJsFileCommand("/a/b/c/server.js")
	fmt.Println(command)
	// Output:
	// node c/server.js
}

func ExampleNodeStartupScriptGenerator_getPackageJsonMainCommand_rootDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	packageJson := &packageJson{
		Main: "server.js",
	}
	command := gen.getPackageJsonMainCommand(packageJson, "/a/b/package.json")
	fmt.Println(command)
	// Output:
	// node server.js
}

func ExampleNodeStartupScriptGenerator_getPackageJsonMainCommand_subDir() {
	gen := &NodeStartupScriptGenerator{
		SourcePath: "/a/b",
	}
	packageJson := &packageJson{
		Main: "server.js",
	}
	command := gen.getPackageJsonMainCommand(packageJson, "/a/b/c/package.json")
	fmt.Println(command)
	// Output:
	// node c/server.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_simpleNodeCommand() {
	gen := &NodeStartupScriptGenerator{}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getConfigJsCommand_returnsEmptyString_WhenUsePm2IsFalse(t *testing.T) {
	gen := &NodeStartupScriptGenerator{
		UsePm2: false,
	}
	command := gen.getConfigJsCommand("ecosystem.config.js")
	assert.Empty(t, command)
}

func ExampleNodeStartupScriptGenerator_getConfigYamlCommand_returnsEmptyString_WhenUsePm2IsFalse(t *testing.T) {
	gen := &NodeStartupScriptGenerator{
		UsePm2: false,
	}
	command := gen.getConfigYamlCommand("ecosystem.config.yaml")
	assert.Empty(t, command)
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_customServerPassedIn() {
	gen := &NodeStartupScriptGenerator{
		UsePm2: true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// pm2 start --no-daemon a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingFlagShouldBeIncluded() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: false,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect=0.0.0.0 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkFlagShouldBeIncluded() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk=0.0.0.0 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingWithHostAndPort() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:     true,
		RemoteDebuggingPort: "1234",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect=0.0.0.0:1234 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkWithHostButNoPort() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk=0.0.0.0 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkWithHostAndPort() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: true,
		RemoteDebuggingPort:             "1234",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk=0.0.0.0:1234 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingLegacyNodeVersion() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: false,
		UseLegacyDebugger:               true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --debug=0.0.0.0 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkLegacyNodeVersion() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: true,
		UseLegacyDebugger:               true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --debug-brk=0.0.0.0 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkWithHostAndPortLegacyNodeVersion() {
	gen := &NodeStartupScriptGenerator{
		RemoteDebugging:                 true,
		RemoteDebuggingBreakBeforeStart: true,
		RemoteDebuggingPort:             "1234",
		UseLegacyDebugger:               true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --debug-brk=0.0.0.0:1234 a/b/c.js
}
