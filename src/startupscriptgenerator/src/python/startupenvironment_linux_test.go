// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"archive/zip"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"syscall"
	"testing"

	"github.com/stretchr/testify/require"
)

type startupEnvironmentFixture struct {
	root       string
	home       string
	hook       string
	events     string
	sites      string
	snapshot   string
	python     string
	version    string
	path       string
	pythonPath string
	generator  PythonStartupScriptGenerator
}

func newStartupEnvironmentFixture(t *testing.T) *startupEnvironmentFixture {
	t.Helper()
	root, err := os.MkdirTemp("", "oryx-startup-")
	require.NoError(t, err)
	t.Cleanup(func() { require.NoError(t, os.RemoveAll(root)) })
	require.NoError(t, os.Chmod(root, 0755))
	python, err := exec.LookPath("python")
	if err != nil {
		python, err = exec.LookPath("python3")
	}
	require.NoError(t, err, "runtime tests require a real Python interpreter with the venv module")
	version, exitCode := runStartupEnvironmentCommand(t, exec.Command(python, "-I", "-S", "-c",
		"import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))"))
	require.Zero(t, exitCode, version)
	home := filepath.Join(root, "home")
	bin := filepath.Join(root, "bin")
	app := filepath.Join(root, "app")
	for _, dir := range []string{home, bin, app} {
		require.NoError(t, os.Mkdir(dir, 0755))
	}
	require.NoError(t, os.Symlink(python, filepath.Join(bin, "python")))
	gen := newStartupEnvironmentGenerator(t)
	gen.AppPath = app
	gen.Manifest.PythonVersion = "test-runtime"
	gen.Configuration.PreRunCommand = "export PRE_RUN_VALUE=done\nprintf 'pre:%s\\n' \"$HOOK_VALUE\" >> \"$TEST_EVENTS\""
	gen.UserStartupCommand = `python -c "import os; open(os.environ['TEST_EVENTS'], 'a').write('app:' + os.environ['HOOK_VALUE'] + ':' + os.environ['PRE_RUN_VALUE'] + '\n')"`
	fixture := &startupEnvironmentFixture{
		root:       root,
		home:       home,
		hook:       filepath.Join(root, "setup.sh"),
		events:     filepath.Join(home, "events"),
		sites:      filepath.Join(home, "customizations"),
		snapshot:   filepath.Join(home, "environment"),
		python:     python,
		version:    strings.TrimSpace(version),
		path:       bin + ":" + os.Getenv("PATH"),
		pythonPath: filepath.Join(root, "python-path"),
		generator:  gen,
	}
	fixture.generator.StartupEnvironmentScript = fixture.hook
	return fixture
}

func (fixture *startupEnvironmentFixture) command(t *testing.T, script string) *exec.Cmd {
	t.Helper()
	scriptPath := filepath.Join(fixture.root, "run.sh")
	require.NoError(t, os.WriteFile(scriptPath, []byte(script), 0644))
	command := exec.Command("/bin/sh", scriptPath)
	command.Env = []string{
		"HOME=" + fixture.home,
		"PATH=" + fixture.path,
		"PYTHONPATH=" + fixture.pythonPath,
		"TEST_EVENTS=" + fixture.events,
		"TEST_CUSTOMIZATIONS=" + fixture.sites,
		"TEST_ENVIRONMENT=" + fixture.snapshot,
	}
	return command
}

func (fixture *startupEnvironmentFixture) writeHook(t *testing.T) {
	t.Helper()
	require.NoError(t, os.WriteFile(fixture.hook, []byte(`if [ -e "$TEST_CUSTOMIZATIONS" ]; then
    echo 'Python customization ran before environment setup.' >&2
    return 91
fi
printf 'hook\n' >> "$TEST_EVENTS"
export HOOK_VALUE=ready
unset PYTHONHOME
printf '%s\n' "$APP_PATH" "$PATH" "${PYTHONPATH-}" "${VIRTUAL_ENV-}" > "$TEST_ENVIRONMENT"
`), 0644))
}

func (fixture *startupEnvironmentFixture) writeCustomizations(t *testing.T) {
	t.Helper()
	userSite := filepath.Join(fixture.home, ".local", "lib", "python"+fixture.version, "site-packages")
	require.NoError(t, os.MkdirAll(userSite, 0755))
	require.NoError(t, os.MkdirAll(fixture.pythonPath, 0755))
	for path, name := range map[string]string{
		filepath.Join(fixture.pythonPath, "sitecustomize.py"): "site",
		filepath.Join(userSite, "usercustomize.py"):           "user",
		filepath.Join(userSite, "startup-test.pth"):           "pth",
	} {
		code := fmt.Sprintf("import os; open(os.environ['TEST_CUSTOMIZATIONS'], 'a').write('%s:' + os.environ.get('HOOK_VALUE', 'unset') + '\\n')\n", name)
		require.NoError(t, os.WriteFile(path, []byte(code), 0644))
	}
}

func TestStartupEnvironmentScriptRuntimeEnvironments(t *testing.T) {
	scenarios := []struct {
		name             string
		virtualEnv       bool
		activate         bool
		packages         bool
		skipExtraction   bool
		compressedVenv   string
		compressedOutput string
	}{
		{name: "no-packages"},
		{name: "package-directory", packages: true},
		{name: "uncompressed-venv-path-only", virtualEnv: true},
		{name: "uncompressed-venv-activated", virtualEnv: true, activate: true},
		{name: "venv-and-package-directory", virtualEnv: true, activate: true, packages: true},
		{name: "externally-extracted-venv", virtualEnv: true, activate: true, skipExtraction: true},
		{name: "compressed-venv-gzip", virtualEnv: true, activate: true, compressedVenv: ".tar.gz"},
		{name: "compressed-venv-zip", virtualEnv: true, activate: true, compressedVenv: ".zip"},
		{name: "compressed-venv-zstd", virtualEnv: true, activate: true, compressedVenv: ".tar.zst"},
		{name: "compressed-output-gzip", virtualEnv: true, activate: true, compressedOutput: ".tar.gz"},
		{name: "compressed-output-zstd", virtualEnv: true, activate: true, compressedOutput: ".tar.zst"},
	}
	for _, scenario := range scenarios {
		t.Run(scenario.name, func(t *testing.T) {
			if scenario.compressedVenv != "" && os.Geteuid() != 0 {
				t.Skip("the existing compressed virtualenv layout extracts to the filesystem root; run in the test container")
			}
			fixture := newStartupEnvironmentFixture(t)
			gen := &fixture.generator
			effectiveAppPath := gen.AppPath
			if scenario.compressedOutput != "" {
				effectiveAppPath = filepath.Join(fixture.root, "extracted")
				require.NoError(t, os.Mkdir(effectiveAppPath, 0755))
				gen.Manifest.CompressDestinationDir = "true"
				gen.Manifest.SourceDirectoryInBuildContainer = effectiveAppPath
			}

			expectedPath := "/opt/python/test-runtime/bin:" + fixture.path
			expectedPythonPath := fixture.pythonPath
			expectedVirtualEnv := ""
			packagePath := filepath.Join(effectiveAppPath, "packages")
			if scenario.virtualEnv {
				venvName := "venv"
				venvPath := filepath.Join(effectiveAppPath, venvName)
				if scenario.compressedVenv != "" {
					var err error
					venvPath, err = os.MkdirTemp("/", "oryx-venv-")
					require.NoError(t, err)
					t.Cleanup(func() { require.NoError(t, os.RemoveAll(venvPath)) })
					venvName = filepath.Base(venvPath)
				}
				output, exitCode := runStartupEnvironmentCommand(t,
					exec.Command(fixture.python, "-I", "-S", "-m", "venv", "--without-pip", "--copies", venvPath))
				require.Zero(t, exitCode, output)
				gen.VirtualEnvName = "unused-command-line-name"
				gen.Manifest.VirtualEnvName = venvName
				sitePackages := filepath.Join(venvPath, "lib", "python"+fixture.version, "site-packages")
				require.NoError(t, os.WriteFile(filepath.Join(sitePackages, "startup_fixture_package.py"), []byte("VALUE = 'ready'\n"), 0644))
				expectedPythonPath += ":" + sitePackages
				if scenario.activate {
					expectedVirtualEnv = venvPath
					expectedPath = filepath.Join(venvPath, "bin") + ":" + expectedPath
					if scenario.compressedOutput == "" {
						gen.Manifest.SourceDirectoryInBuildContainer = filepath.Join(fixture.root, "build")
					}
				}
				if scenario.compressedVenv != "" {
					gen.Manifest.CompressedVirtualEnvFile = venvName + scenario.compressedVenv
					createStartupEnvironmentArchive(t, venvPath,
						filepath.Join(gen.AppPath, gen.Manifest.CompressedVirtualEnvFile), scenario.compressedVenv)
					require.NoError(t, os.RemoveAll(venvPath))
				}
				if scenario.skipExtraction {
					gen.Manifest.CompressedVirtualEnvFile = venvName + ".tar.gz"
					gen.SkipVirtualEnvExtraction = true
				}
			}
			if scenario.packages {
				require.NoError(t, os.MkdirAll(filepath.Join(packagePath, "bin"), 0755))
				require.NoError(t, os.WriteFile(filepath.Join(packagePath, "startup_fixture_package.py"), []byte("VALUE = 'ready'\n"), 0644))
				gen.PackageDirectory = "unused-command-line-packages"
				gen.Manifest.PackageDir = "packages"
				expectedPath = filepath.Join(packagePath, "bin") + ":" + expectedPath
			}
			if scenario.packages || scenario.virtualEnv {
				gen.UserStartupCommand = strings.Replace(gen.UserStartupCommand, "import os;",
					"import os, startup_fixture_package; assert startup_fixture_package.VALUE == 'ready';", 1)
			}
			if scenario.compressedOutput != "" {
				createStartupEnvironmentArchive(t, effectiveAppPath,
					filepath.Join(gen.AppPath, "output"+scenario.compressedOutput), scenario.compressedOutput)
				require.NoError(t, os.RemoveAll(effectiveAppPath))
			}
			fixture.writeCustomizations(t)
			script := gen.GenerateEntrypointScript()
			_, err := os.Stat(fixture.events)
			require.True(t, os.IsNotExist(err), "generation must not execute the hook")
			fixture.writeHook(t)
			command := fixture.command(t, script)
			command.Env = append(command.Env, "PYTHONHOME="+filepath.Join(fixture.root, "invalid-python-home"))
			output, exitCode := runStartupEnvironmentCommand(t, command)
			require.Zero(t, exitCode, output)
			events, err := os.ReadFile(fixture.events)
			require.NoError(t, err, output)
			require.Equal(t, "hook\npre:ready\napp:ready:done\n", string(events))
			snapshot, err := os.ReadFile(fixture.snapshot)
			require.NoError(t, err, output)
			require.Equal(t, strings.Join([]string{effectiveAppPath, expectedPath, expectedPythonPath, expectedVirtualEnv, ""}, "\n"), string(snapshot))
			sites, err := os.ReadFile(fixture.sites)
			require.NoError(t, err, "the real application interpreter should load customization after the hook")
			require.Contains(t, string(sites), "site:ready\n")
			require.NotContains(t, string(sites), "unset")
			if scenario.packages && !scenario.activate {
				require.Contains(t, string(sites), "pth:ready\n")
				require.Contains(t, string(sites), "user:ready\n")
			}
			if scenario.packages {
				pth, err := os.ReadFile(filepath.Join(fixture.home, ".local", "lib", "python"+fixture.version, "site-packages", "oryx.pth"))
				require.NoError(t, err)
				require.Equal(t, packagePath+"\n", string(pth))
			}
		})
	}
}

func TestStartupEnvironmentScriptRuntimeLiteralPath(t *testing.T) {
	fixture := newStartupEnvironmentFixture(t)
	fixture.hook = filepath.Join(fixture.root, "host's $(touch injected-one); `touch injected-two` \"$HOME\" \\\n setup.sh")
	fixture.generator.StartupEnvironmentScript = fixture.hook
	fixture.writeHook(t)
	script := fixture.generator.GenerateEntrypointScript()
	_, err := os.Stat(fixture.events)
	require.True(t, os.IsNotExist(err))
	output, exitCode := runStartupEnvironmentCommand(t, fixture.command(t, script))
	require.Zero(t, exitCode, output)
	events, err := os.ReadFile(fixture.events)
	require.NoError(t, err)
	require.Equal(t, "hook\npre:ready\napp:ready:done\n", string(events))
	for _, name := range []string{"injected-one", "injected-two"} {
		_, err := os.Stat(filepath.Join(fixture.generator.AppPath, name))
		require.True(t, os.IsNotExist(err), "the path must never execute shell substitutions")
	}
}

func TestStartupEnvironmentScriptRuntimeFailures(t *testing.T) {
	for _, scenario := range []struct {
		name     string
		content  string
		mode     os.FileMode
		exitCode int
	}{
		{name: "missing", exitCode: 2},
		{name: "unreadable", content: "return 0\n", mode: 0000, exitCode: 2},
		{name: "nonzero-return", content: "echo 'setup declined' >&2\nreturn 37\n", mode: 0644, exitCode: 37},
		{name: "explicit-exit", content: "echo 'setup declined' >&2\nexit 29\n", mode: 0644, exitCode: 29},
		{name: "last-command-failure", content: "echo 'setup declined' >&2\nfalse\n", mode: 0644, exitCode: 1},
	} {
		t.Run(scenario.name, func(t *testing.T) {
			fixture := newStartupEnvironmentFixture(t)
			if scenario.content != "" {
				require.NoError(t, os.WriteFile(fixture.hook, []byte(scenario.content), scenario.mode))
			}
			command := fixture.command(t, fixture.generator.GenerateEntrypointScript())
			if scenario.name == "unreadable" && os.Geteuid() == 0 {
				require.NoError(t, os.Chmod(fixture.home, 0777))
				command.SysProcAttr = &syscall.SysProcAttr{Credential: &syscall.Credential{Uid: 65534, Gid: 65534}}
			}
			output, exitCode := runStartupEnvironmentCommand(t, command)
			require.Equal(t, scenario.exitCode, exitCode, output)
			require.Contains(t, output, "Running the startup environment script...")
			if scenario.name == "missing" || scenario.name == "unreadable" {
				require.Contains(t, output, fixture.hook)
			} else {
				require.Contains(t, output, "setup declined")
			}
			_, err := os.Stat(fixture.events)
			require.True(t, os.IsNotExist(err), "neither pre-run nor app should run:\n%s", output)
		})
	}
}

func createStartupEnvironmentArchive(t *testing.T, source string, destination string, extension string) {
	t.Helper()
	if extension != ".zip" {
		args := []string{"-czf", destination, "-C", source, "."}
		if extension == ".tar.zst" {
			args = []string{"-I", "zstd", "-cf", destination, "-C", source, "."}
		}
		output, exitCode := runStartupEnvironmentCommand(t, exec.Command("tar", args...))
		require.Zero(t, exitCode, output)
		return
	}

	file, err := os.Create(destination)
	require.NoError(t, err)
	writer := zip.NewWriter(file)
	err = filepath.Walk(source, func(path string, info os.FileInfo, err error) error {
		if err != nil || path == source {
			return err
		}
		header, err := zip.FileInfoHeader(info)
		if err != nil {
			return err
		}
		header.Name, err = filepath.Rel(source, path)
		if err != nil {
			return err
		}
		if info.IsDir() {
			header.Name += "/"
		}
		entry, err := writer.CreateHeader(header)
		if err != nil || info.IsDir() {
			return err
		}
		if info.Mode()&os.ModeSymlink != 0 {
			target, err := os.Readlink(path)
			if err != nil {
				return err
			}
			_, err = io.WriteString(entry, target)
			return err
		}
		data, err := os.ReadFile(path)
		if err != nil {
			return err
		}
		_, err = entry.Write(data)
		return err
	})
	require.NoError(t, err)
	require.NoError(t, writer.Close())
	require.NoError(t, file.Close())
}
