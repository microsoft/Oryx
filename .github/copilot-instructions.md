# Microsoft Oryx Build System

Microsoft Oryx is a build system that automatically compiles source code repos into runnable artifacts for Azure App Service and other platforms. It consists of build images, runtime images, and script generators written in .NET Core and Go.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites
Ensure these tools are installed before working with the codebase:
- **.NET SDK 8.0+**: `dotnet --version` should show 8.0.119 or later
- **Go 1.24+**: `go version` should show go1.24.6 or later  
- **Docker**: `docker --version` for container builds
- **Bash 5.2+**: Required for build scripts
- **yq**: YAML processor - automatically installed by build scripts

### Core Build Process
**CRITICAL BUILD ISSUE**: The main solution (Oryx.sln) has a known dependency issue with `Microsoft.NETCore.App.Host.ubuntu.24.04-x64` package. Use individual project builds instead:

Build individual components that work reliably:
- `time dotnet build src/BuildScriptGenerator/BuildScriptGenerator.csproj -c Debug` -- takes 19 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
- `time dotnet build Detector.sln -c Debug` -- takes 8 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- `time dotnet build src/BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj -c Debug --runtime linux-x64` -- takes 19 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

**DO NOT** attempt to build the full Oryx.sln - it will fail with ubuntu.24.04-x64 package error.

### Testing
Run tests for components that work:
- `time ./build/testBuildScriptGenerator.sh` -- BuildScriptGenerator tests take 18 seconds, CLI tests fail due to dependency issue. NEVER CANCEL. Set timeout to 60+ seconds.
- `time ./build/testStartupScriptGenerators.sh` -- takes 39 seconds including Docker image downloads. NEVER CANCEL. Set timeout to 90+ seconds.

**NEVER CANCEL builds or tests** - Some operations may appear to hang but are downloading Docker images or compiling large codebases.

### Manual Validation

After making changes, ALWAYS manually validate using these scenarios:

1. **Test platform detection**:
   ```bash
   ./src/BuildScriptGeneratorCli/bin/Debug/linux-x64/GenerateBuildScript detect tests/SampleApps/nodejs/node-mysql/
   ./src/BuildScriptGeneratorCli/bin/Debug/linux-x64/GenerateBuildScript detect tests/SampleApps/python/flask-chatterbot/
   ```

2. **Verify CLI help works**:
   ```bash
   ./src/BuildScriptGeneratorCli/bin/Debug/linux-x64/GenerateBuildScript --help
   ```

3. **Test Go startup script generators**:
   ```bash
   cd src/startupscriptgenerator/src && go test ./...
   ```

Expected CLI detection output format:
```
Platform: nodejs/python
PlatformVersion: Not Detected (expected outside container)
Frameworks: [detected frameworks]
```

### Container-Based Development
Oryx is designed to run in containers with platform SDKs installed. CLI commands expecting `/opt/dotnet`, `/opt/nodejs` directories will fail outside containers - this is expected behavior.

For Docker image builds:
- `cd images && ./build/build_buildimages.sh [image_type] [debian_flavor]` -- takes 15-45 minutes depending on image. NEVER CANCEL. Set timeout to 90+ minutes.
- `cd images && ./runtime/build_runtime_images.sh [stack] [version] [debian_flavor]` -- takes 10-30 minutes. NEVER CANCEL. Set timeout to 60+ minutes.

### Repository Structure
- `src/BuildScriptGenerator/` - Core build script generation logic (.NET Core)
- `src/BuildScriptGeneratorCli/` - Command-line interface (.NET Core)  
- `src/startupscriptgenerator/` - Runtime startup script generators (Go)
- `src/BuildServer/` - HTTP API for build operations (.NET Core)
- `src/Detector/` - Platform detection logic (.NET Core)
- `tests/` - Test projects and sample applications
- `images/` - Docker build and runtime image definitions
- `build/` - Build scripts and tooling

### Validation Steps Before Committing
ALWAYS run these commands before committing changes:
1. Build affected components individually (see Core Build Process above)
2. Run startup script generator tests: `./build/testStartupScriptGenerators.sh`
3. Test CLI functionality with sample apps (see Manual Validation above)
4. If working on specific platforms, test with relevant sample apps in `tests/SampleApps/`

### Known Limitations
- **Full solution build fails** due to ubuntu.24.04-x64 dependency issue - use individual project builds
- **Integration tests fail** due to CLI dependency on above issue
- **Platform listing command fails** outside container environment (expected)
- **Docker builds require significant time** - plan for 15-90 minute builds
- **Some .NET tests have validation warnings** but still complete successfully

### Common Commands Reference
Quick reference for frequently needed operations:

#### Building Core Components
```bash
# Clean everything
dotnet clean Detector.sln

# Build detector (fastest, 8 seconds)
dotnet build Detector.sln -c Debug

# Build main generator (19 seconds)  
dotnet build src/BuildScriptGenerator/BuildScriptGenerator.csproj -c Debug

# Build CLI tool (19 seconds)
dotnet build src/BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj -c Debug --runtime linux-x64
```

#### Testing
```bash
# Test startup generators (39 seconds)
./build/testStartupScriptGenerators.sh

# Test build script generator (18 seconds, may show CLI errors)
./build/testBuildScriptGenerator.sh
```

#### Platform Detection
```bash
# Test Node.js detection
./src/BuildScriptGeneratorCli/bin/Debug/linux-x64/GenerateBuildScript detect tests/SampleApps/nodejs/node-mysql/

# Test Python detection  
./src/BuildScriptGeneratorCli/bin/Debug/linux-x64/GenerateBuildScript detect tests/SampleApps/python/flask-chatterbot/
```

Remember: NEVER CANCEL long-running operations. This system builds container images and large .NET solutions that require patience.