package main

import "flag"
import "os"
import "path/filepath"

func main() {
	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	defaultAppFilePathPtr := flag.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/startup/default-static-site.js'")
	customStartCommandPtr := flag.String("serverCmd", "", "[Optional] Command to start the server, if different than 'node', e.g. 'pm2 start --no-daemon'")
	remoteDebugEnabledPtr := flag.Bool("remoteDebug", false, "Application will run in debug mode.")
	remoteDebugBrkEnabledPtr := flag.Bool("remoteDebugBrk", false, "Application will run in debug mode, and will debugger will break before the user code starts.")
	remoteDebugIp := flag.String("debugHost", "", "The IP address where the debugger will listen to, e.g. '0.0.0.0' or '127.0.0.1")
	remoteDebugPort := flag.String("debugPort", "", "The port the debugger will listen to.")
	flag.Parse()
	
	fullAppPath, err := filepath.Abs(*appPathPtr)
	if err != nil {
		println(err)
		return
	}

	if _,err := os.Stat(fullAppPath); os.IsNotExist(err) {
		panic("Path '" + fullAppPath + "' does not exist.")
	}
	
	// Validate the default app file.
	if *defaultAppFilePathPtr != "" {
		fullPath, err := filepath.Abs(*defaultAppFilePathPtr)
		if err != nil {
			panic(err)
		}
		*defaultAppFilePathPtr = fullPath

		if _,err := os.Stat(*defaultAppFilePathPtr); os.IsNotExist(err) {
			panic("Couldn't find file '" + *defaultAppFilePathPtr + "'")
		}
	}
	
	gen := NodeStartupScriptGenerator{
		SourcePath: fullAppPath,
		DefaultAppJsFilePath: *defaultAppFilePathPtr,
		CustomStartCommand: *customStartCommandPtr,
		RemoteDebugging: *remoteDebugEnabledPtr,
		RemoteDebuggingBreakBeforeStart: *remoteDebugBrkEnabledPtr,
		RemoteDebuggingIp: *remoteDebugIp,
		RemoteDebuggingPort: *remoteDebugPort,
	}
	script := gen.GenerateEntrypointCommand()
	println(script)
}