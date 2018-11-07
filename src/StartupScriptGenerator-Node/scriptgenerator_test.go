package main

import (
	"fmt"
)

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_simple() {
	gen := &NodeStartupScriptGenerator {
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_customServer() {
	gen := &NodeStartupScriptGenerator {
		CustomStartCommand: "pm2 start --no-daemon",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// pm2 start --no-daemon a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debugging() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingBreakBeforeStart: false,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrk() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingBreakBeforeStart: true,
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingWithHost() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingIp: "0.1.2.3",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect=0.1.2.3 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingWithHostAndPort() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingIp: "0.1.2.3",
		RemoteDebuggingPort: "1234",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect=0.1.2.3:1234 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkWithHost() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingBreakBeforeStart: true,
		RemoteDebuggingIp: "0.1.2.3",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk=0.1.2.3 a/b/c.js
}

func ExampleNodeStartupScriptGenerator_getStartupCommandFromJsFile_debuggingbrkWithHostAndPort() {
	gen := &NodeStartupScriptGenerator {
		RemoteDebugging: true,
		RemoteDebuggingBreakBeforeStart: true,
		RemoteDebuggingIp: "0.1.2.3",
		RemoteDebuggingPort: "1234",
	}
	command := gen.getStartupCommandFromJsFile("a/b/c.js")
	fmt.Println(command)
	// Output:
	// node --inspect-brk=0.1.2.3:1234 a/b/c.js
}