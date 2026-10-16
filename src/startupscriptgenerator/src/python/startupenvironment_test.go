// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"github.com/stretchr/testify/require"
)

func newStartupEnvironmentGenerator(t *testing.T) PythonStartupScriptGenerator {
	t.Helper()
	t.Setenv(consts.ApplicationInsightsConnectionStringEnvVarName, "")
	t.Setenv(consts.PythonGunicornConfigPathEnvVarName, "")
	t.Setenv(consts.PythonEnableGunicornMultiWorkersEnvVarName, "false")
	return PythonStartupScriptGenerator{
		AppPath:               t.TempDir(),
		DefaultAppPath:        "/default",
		DefaultAppModule:      "application:app",
		DefaultAppDebugModule: "application",
		DebugPort:             "5678",
		Manifest:              common.BuildManifest{PythonVersion: "3.12.0"},
	}
}

func TestStartupEnvironmentScriptLegacyOutput(t *testing.T) {
	const template = `#!/bin/sh

echo 'export APP_PATH="{{APP_PATH}}"' >> ~/.bashrc
echo 'cd $APP_PATH' >> ~/.bashrc
{{PRE_RUN}}
# Enter the source directory to make sure the script runs where the user expects
cd {{APP_PATH}}

export APP_PATH="{{APP_PATH}}"
if [ -z "$HOST" ]; then
		export HOST=0.0.0.0
fi

if [ -z "$PORT" ]; then
		export PORT=80
fi

export PATH="/opt/python/3.12.0/bin:${PATH}"
{{PACKAGE_SETUP}}{{COMMAND}}
`
	const packageSetup = `echo Using package directory '{{APP_PATH}}/packages'
SITE_PACKAGE_PYTHON_VERSION=$(python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
SITE_PACKAGES_PATH=$HOME"/.local/lib/python"$SITE_PACKAGE_PYTHON_VERSION"/site-packages"
mkdir -p $SITE_PACKAGES_PATH
echo "{{APP_PATH}}/packages" > $SITE_PACKAGES_PATH"/oryx.pth"
PATH="{{APP_PATH}}/packages/bin:$PATH"
echo "Updated PATH to '$PATH'"
`
	const preRun = `cd "{{APP_PATH}}"
echo 'Running the provided pre-run command...'
printf 'pre-run\n'
# End of pre-run command.
`
	for _, appCommand := range []string{"default", "custom", "debug"} {
		for _, packages := range []bool{false, true} {
			for _, withPreRun := range []bool{false, true} {
				t.Run(fmt.Sprintf("%s/packages=%t/pre-run=%t", appCommand, packages, withPreRun), func(t *testing.T) {
					gen := newStartupEnvironmentGenerator(t)
					expected := strings.ReplaceAll(template, "{{PACKAGE_SETUP}}", "")
					if packages {
						gen.PackageDirectory = "packages"
						require.NoError(t, os.Mkdir(filepath.Join(gen.AppPath, gen.PackageDirectory), 0755))
						expected = strings.ReplaceAll(template, "{{PACKAGE_SETUP}}", packageSetup)
					}
					if withPreRun {
						gen.Configuration.PreRunCommand = "printf 'pre-run\\n'"
						expected = strings.ReplaceAll(expected, "{{PRE_RUN}}", preRun)
					} else {
						expected = strings.ReplaceAll(expected, "{{PRE_RUN}}", "")
					}
					command := `GUNICORN_CMD_ARGS="--timeout 600 --access-logfile '-' --error-logfile '-' --chdir=/default" gunicorn application:app`
					switch appCommand {
					case "custom":
						gen.UserStartupCommand = "printf 'app\\n'"
						command = `PATH="$PATH:{{APP_PATH}}" printf 'app\n'`
					case "debug":
						gen.DebugAdapter = "debugpy"
						command = "cd /default && python -m debugpy --listen 0.0.0.0:5678  -m application"
					}
					expected = strings.ReplaceAll(expected, "{{COMMAND}}", command)
					expected = strings.ReplaceAll(expected, "{{APP_PATH}}", gen.AppPath)
					require.Equal(t, expected, gen.GenerateEntrypointScript())
				})
			}
		}
	}
}

func TestStartupEnvironmentScriptOrdering(t *testing.T) {
	for _, appCommand := range []string{"default", "custom", "debugpy", "ptvsd"} {
		t.Run(appCommand, func(t *testing.T) {
			gen := newStartupEnvironmentGenerator(t)
			gen.StartupEnvironmentScript = "/runtime/setup.sh"
			gen.VirtualEnvName = "venv"
			gen.Manifest.SourceDirectoryInBuildContainer = filepath.Join(gen.AppPath, "build")
			gen.PackageDirectory = "packages"
			gen.Configuration.PreRunCommand = "printf 'pre-run\\n'"
			require.NoError(t, os.Mkdir(filepath.Join(gen.AppPath, "venv"), 0755))
			require.NoError(t, os.Mkdir(filepath.Join(gen.AppPath, "packages"), 0755))
			expectedCommand := "gunicorn application:app"
			switch appCommand {
			case "custom":
				gen.UserStartupCommand = "printf 'app\\n'"
				expectedCommand = gen.UserStartupCommand
			case "debugpy", "ptvsd":
				gen.DebugAdapter = appCommand
				gen.DebugWait = true
				expectedCommand = "python -m debugpy --listen 0.0.0.0:5678 --wait-for-client -m application"
			}

			script := gen.GenerateEntrypointScript()
			require.True(t, strings.HasPrefix(script, "#!/bin/sh\n"))
			assertStartupEnvironmentOrder(t, script,
				"export APP_PATH=",
				"export PATH=",
				"PYTHON_VERSION=$(python -I -S -c ",
				". venv/bin/activate\n",
				"SITE_PACKAGE_PYTHON_VERSION=$(python -I -S -c ",
				`echo "Updated PATH to '$PATH'"`,
				". '/runtime/setup.sh' || exit \"$?\"\n",
				gen.Configuration.PreRunCommand,
				expectedCommand)
			require.Equal(t, 1, strings.Count(script, gen.Configuration.PreRunCommand))
			require.Equal(t, 1, strings.Count(script, ". '/runtime/setup.sh'"))
		})
	}
}

func TestStartupEnvironmentScriptQuotesLiteralPath(t *testing.T) {
	gen := newStartupEnvironmentGenerator(t)
	gen.StartupEnvironmentScript = "/runtime/host's $(false); \"setup\".sh"
	script := gen.GenerateEntrypointScript()
	require.Contains(t, script, ". '/runtime/host'\\''s $(false); \"setup\".sh' || exit \"$?\"\n")
}

func TestStartupEnvironmentScriptLegacyVirtualEnvironmentProbe(t *testing.T) {
	gen := newStartupEnvironmentGenerator(t)
	gen.VirtualEnvName = "venv"
	gen.Configuration.PreRunCommand = "printf 'pre-run\\n'"
	require.NoError(t, os.Mkdir(filepath.Join(gen.AppPath, "venv"), 0755))
	script := gen.GenerateEntrypointScript()
	require.NotContains(t, script, " -I -S ")
	require.NotContains(t, script, "startup environment script")
	assertStartupEnvironmentOrder(t, script, gen.Configuration.PreRunCommand,
		"export APP_PATH=", "PYTHON_VERSION=$(python -c ", "gunicorn application:app")
}

func TestStartupEnvironmentScriptCLI(t *testing.T) {
	for _, scriptPath := range []string{"setup.sh", "./setup.sh", "../setup.sh", "~/setup.sh", `C:\setup.sh`} {
		t.Run("rejects "+scriptPath, func(t *testing.T) {
			outputPath := filepath.Join(t.TempDir(), "run.sh")
			output, exitCode := runPythonStartupCLI(t, "create-script",
				"-startupEnvironmentScript", scriptPath, "-output", outputPath)
			require.Equal(t, consts.FAILURE_EXIT_CODE, exitCode, output)
			require.Contains(t, output, "-startupEnvironmentScript must be an absolute POSIX path")
			_, err := os.Stat(outputPath)
			require.True(t, os.IsNotExist(err))
		})
	}

	t.Run("absolute runtime-only path", func(t *testing.T) {
		appPath := t.TempDir()
		outputPath := filepath.Join(t.TempDir(), "run.sh")
		output, exitCode := runPythonStartupCLI(t, "create-script", "-appPath", appPath,
			"-defaultApp", appPath, "-startupEnvironmentScript", "/runtime/host's setup.sh",
			"-output", outputPath)
		require.Zero(t, exitCode, output)
		script, err := os.ReadFile(outputPath)
		require.NoError(t, err)
		require.Contains(t, string(script), ". '/runtime/host'\\''s setup.sh' || exit \"$?\"\n")
	})

	t.Run("empty option is identical to omitted option", func(t *testing.T) {
		appPath := t.TempDir()
		outputPath := filepath.Join(t.TempDir(), "run.sh")
		args := []string{"create-script", "-appPath", appPath, "-defaultApp", appPath, "-output", outputPath}
		output, exitCode := runPythonStartupCLI(t, args...)
		require.Zero(t, exitCode, output)
		withoutOption, err := os.ReadFile(outputPath)
		require.NoError(t, err)
		output, exitCode = runPythonStartupCLI(t, append(args, "-startupEnvironmentScript", "")...)
		require.Zero(t, exitCode, output)
		withEmptyOption, err := os.ReadFile(outputPath)
		require.NoError(t, err)
		require.Equal(t, withoutOption, withEmptyOption)
	})
}

func TestPythonStartupCLIProcess(t *testing.T) {
	if os.Getenv("ORYX_TEST_PYTHON_CLI") != "1" {
		return
	}
	for i, arg := range os.Args {
		if arg == "--" {
			os.Args = append([]string{os.Args[0]}, os.Args[i+1:]...)
			main()
			os.Exit(0)
		}
	}
	t.Fatal("missing CLI argument separator")
}

func runPythonStartupCLI(t *testing.T, args ...string) (string, int) {
	t.Helper()
	executable, err := os.Executable()
	require.NoError(t, err)
	command := exec.Command(executable, append([]string{"-test.run=^TestPythonStartupCLIProcess$", "--"}, args...)...)
	command.Env = append(os.Environ(), "ORYX_TEST_PYTHON_CLI=1",
		"ORYX_AI_CONNECTION_STRING=", "ENABLE_DYNAMIC_INSTALL=false", "PYTHON_VERSION=3.12.0", "PRE_RUN_COMMAND=")
	return runStartupEnvironmentCommand(t, command)
}

func runStartupEnvironmentCommand(t *testing.T, command *exec.Cmd) (string, int) {
	t.Helper()
	output, err := command.CombinedOutput()
	if err == nil {
		return string(output), 0
	}
	var exitError *exec.ExitError
	require.True(t, errors.As(err, &exitError), "could not run %v: %v\n%s", command.Args, err, output)
	return string(output), exitError.ExitCode()
}

func assertStartupEnvironmentOrder(t *testing.T, script string, parts ...string) {
	t.Helper()
	offset := 0
	for _, part := range parts {
		index := strings.Index(script[offset:], part)
		require.NotEqual(t, -1, index, "missing or out-of-order %q in:\n%s", part, script)
		offset += index + len(part)
	}
}
