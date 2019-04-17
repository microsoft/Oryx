// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_simpleNodeCommand() {
	gen := &NodeStartupScriptGenerator{}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_customServerPassedIn() {
	gen := &NodeStartupScriptGenerator{
		CustomStartCommand: "pm2 start --no-daemon",
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
