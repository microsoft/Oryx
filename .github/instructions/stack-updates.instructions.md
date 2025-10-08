# Stack Updates Workflow Instructions for Oryx

This document provides comprehensive instructions for AI assistants working with the Oryx repository to handle stack version updates. This applies to all supported stacks: dotnet, node (nodejs), python, php

## Overview

The Oryx stack update workflow involves two main components:
1. **SDK Publishing**: Building and publishing SDK artifacts to Azure storage accounts
2. **Runtime Image Building**: Creating runtime images that reference the published SDKs

## Key Files and Structure

### 1. Central Configuration - `images/constants.yml`
- **Purpose**: Central version configuration for runtime image building
- **Content**: Version numbers, SHA256 hashes, GPG keys, and OS flavor mappings
- **Format**:
  ```yaml
  node18Version: 18.20.8
  node20Version: 20.19.3
  node22Version: 22.17.0
  php81Version: 8.1.33
  php81Version_SHA: 9db83bf4590375562bc1a10b353cccbcf9fcfc56c58b7c8fb814e6865bb928d1
  php81_GPG_keys: "528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544"
  php82_GPG_keys: "1198C0117593497A5EC5C199286AF1F9897469DC 39B641343D8C104B2B146DC3F9C39DC0B9698544"
  php83_GPG_keys: "1198C0117593497A5EC5C199286AF1F9897469DC AFD8691FDAEDF03BDF6E460563F15A9B715376CA"
  php84_GPG_keys: "AFD8691FDAEDF03BDF6E460563F15A9B715376CA 9D7F99A0CB8F05C8A6958D6256A97AF7600A39A6"
  python310_GPG_keys: A035C8C19219BA821ECEA86B64E628F8D684696D
  python311_GPG_keys: A035C8C19219BA821ECEA86B64E628F8D684696D
  python39Version: 3.9.23
  python310Version: 3.10.18
  ASPNET_CORE_APP_80: 8.0.18
  ASPNET_CORE_APP_80_SHA: 896e9cab7c3ea5384c174e7e2cffae3c7f8f9ed5d6d2b7434b5a2b0dc3f02b611ff8668f5d70c0b356a6a5d85a28fe40756cf356b168d0306370da11646b4b23
  NET_CORE_APP_80: 8.0.18
  NET_CORE_APP_80_SHA: 15d754a01c93183ea98bd608f2691193c86f284ec7feddfc810fad919e2f7ba20d41e1de45789fc1d9ac9fcd8be82d49cb8fe4c471dec892f91272fea2e53f08 
  ```

### 2. Platform-Specific Version Files - `platforms/{stack}/versions/{os-flavor}/versionsToBuild.txt`
- **Purpose**: Define which versions should be built and published for SDK generation
- **Target OS Flavors**: The specific OS flavors to update are determined by the corresponding `{stackVersion}osFlavors` property in `constants.yml`
- **Workflow**: When updating a stack version (e.g., Node.js 18), reference `node18osFlavors` in `constants.yml` to identify which OS flavor directories require `versionsToBuild.txt` updates
- **File Structure**:
  ```
  platforms/{stack}/versions/{os-flavor}/versionsToBuild.txt
  ```
- **Examples**:
  - For Node.js 20: Update files in OS flavors listed in `node20osFlavors` (typically `bullseye,bookworm`)
  - For Python 3.14: Update files in OS flavors listed in `python314osFlavors` (typically `noble`)
  - For .NET 9.0: Update files in OS flavors listed in `dotnet90osFlavors` (typically `bookworm`)

## OS Flavor Mapping

The repository supports multiple Debian/Ubuntu flavors:
- **bullseye**: Debian 11 (current)
- **bookworm**: Debian 12 (current)
- **noble**: Ubuntu 24.04 (latest)

## Stack Update Workflow

### Step 1: Update versionsToBuild.txt Files

1. **Identify Target Versions**: Determine which stack versions need to be added/updated
2. **Check OS Flavor Support**: Reference the osFlavors mapping in constants.yml to determine which OS flavors support the target version
3. **Update Each OS Flavor File**: For each supported OS flavor, update the corresponding versionsToBuild.txt file with the new version:

   **Node.js Format** (simple version list):
   ```
   18.20.8
   20.19.3
   22.17.0
   ```

   **PHP Format** (version, SHA256, GPG keys - trailing comma required):
   ```
   8.1.33, 9db83bf4590375562bc1a10b353cccbcf9fcfc56c58b7c8fb814e6865bb928d1, 528995BFEDFBA7191D46839EF9BA0ADA31CBD89E 39B641343D8C104B2B146DC3F9C39DC0B9698544,
   8.4.10, 475f991afd2d5b901fb410be407d929bc00c46285d3f439a02c59e8b6fe3589c, 1198C0117593497A5EC5C199286AF1F9897469DC 39B641343D8C104B2B146DC3F9C39DC0B9698544,
   ```

   **Python Format** (version, SHA256):
   ```
   3.11.13, fedcba0987654321abc123def456789012345678901234567890123456789012
   3.12.8, 1234567890abcdef123456789012345678901234567890123456789012345678
   ```

   **.NET Format** (SDK version, SHA256):
   ```
   8.0.403, 15d754a01c93183ea98bd608f2691193c86f284ec7feddfc810fad919e2f7ba20d41e1de45789fc1d9ac9fcd8be82d49cb8fe4c471dec892f91272fea2e53f08
   9.0.100, e273b592ae9e1c75e91ce3be6f4f2d23143276900141e29c673d46490118d0115f6d1968ae6cfed598ca300fe889ba20fa060787ebb540308433580a3c6c5cd8
   ```
   
   **Important .NET Notes**:
   - **SDK versions** (e.g., 9.0.100) go in versionsToBuild.txt
   - **TWO runtime versions** go in constants.yml: `NET_CORE_APP_90` and `ASPNET_CORE_APP_90`
   - Runtime versions may differ from each other (e.g., both could be 9.0.7)
   - Each runtime component has its own SHA256 hash

   **Important Notes**:
   - Maintain chronological or logical version ordering
   - Verify SHA256 hashes against official sources before adding
   - Ensure GPG keys are current and valid

### Step 2: Update constants.yml for Runtime Images

1. **Version Updates**: Update the central version declarations:
   ```yaml
   node22Version: 22.17.0 
   php84Version: 8.4.10
   python314Version: 3.14.0rc2
   # .NET has TWO runtime components (versions may differ):
   NET_CORE_APP_90: 9.0.7        # .NET Core runtime version
   ASPNET_CORE_APP_90: 9.0.7     # ASP.NET Core runtime version
   ```

2. **SHA256 Updates**: Update corresponding SHA256 hashes:
   ```yaml
   php84Version_SHA: 14983a9ef8800e6bc2d920739fd386054402f7976ca9cd7f711509496f0d2632
   # .NET requires BOTH runtime component SHAs (usually different):
   NET_CORE_APP_90_SHA: e273b592ae9e1c75e91ce3be6f4f2d23143276900141e29c673d46490118d0115f6d1968ae6cfed598ca300fe889ba20fa060787ebb540308433580a3c6c5cd8
   ASPNET_CORE_APP_90_SHA: b175d4d0578f9f5d735d59def3f44459462ef36b4bd07d9ca0ed9853bf42d90c7bace195ff264a9dcf6dd4d6d452c87059085146268fce3540ed58eaf39629eb
   ```

3. **OS Flavor Mapping**: Update the osFlavors declarations if adding support for new OS versions:
   ```yaml
   node22osFlavors: bullseye,bookworm,noble  # Added noble support
   ```

### Step 3: Update Test Version Constants

Update version constants in BuildScriptGenerator source files for test compatibility:

- **Node.js**: `src/BuildScriptGenerator/Node/NodeVersions.cs`
- **PHP**: `src/BuildScriptGenerator/PhpVersions.cs`  
- **Python**: `src/BuildScriptGenerator/PythonVersions.cs`
- **.NET**: Update both files with **separate versions**:
  - `src/BuildScriptGenerator/DotNetCore/DotNetCoreRunTimeVersions.cs` - Add **runtime versions**
  - `src/BuildScriptGenerator/DotNetCore/DotNetCoreSdkVersions.cs` - Add **SDK versions**

Add new version strings to the version arrays in these files to ensure tests recognize the new versions. **Important: .NET runtime and SDK versions are different!**

### Step 4: Trigger SDK Build and Publication

1. **Build Process**: The build system reads versionsToBuild.txt files to determine what to build
2. **Storage Publishing**: SDKs are built and published to Azure storage accounts
3. **Verification**: Built artifacts are verified using SHA256 hashes and/or GPG signatures

### Step 5: Runtime Image Building

1. **Dockerfile Updates**: Runtime Dockerfiles in `images/runtime/` reference constants.yml variables
2. **Multi-Platform Builds**: Images are built for supported OS flavors
3. **Registry Publishing**: Runtime images are published to container registries

## Best Practices

### Version Management
- **Incremental Updates**: Add new versions while maintaining existing ones for backward compatibility
- **Security Patches**: Prioritize security patch versions
- **LTS Versions**: Give priority to Long Term Support versions

### File Maintenance
- **Consistent Formatting**: Maintain the exact format expected by each stack's build scripts
- **Hash Verification**: Always include and verify SHA256 hashes for security
- **GPG Key Management**: For PHP, ensure GPG keys are current and valid

### Testing Strategy
- **Development Environment**: Test changes in development storage accounts first
- **OS Flavor Coverage**: Ensure all supported OS flavors are updated consistently
- **Build Validation**: Verify that builds complete successfully before runtime updates

## Common Tasks

### Adding a New Stack Version

**General Process for Any Stack (Node.js, PHP, Python, .NET):**

1. **Check OS Flavor Support**: Look up `{stack}{version}osFlavors` in `constants.yml`
2. **Update versionsToBuild.txt files**: Add version to each supported OS flavor
3. **Update constants.yml**: Add version and SHA256 (if required)
4. **Update Test Version Constants**: Update version constants in BuildScriptGenerator source files

#### Node.js Example
```bash
# 1. Check constants.yml: node20osFlavors: bullseye,bookworm
# 2. Update versionsToBuild.txt files
echo "20.20.0" >> platforms/nodejs/versions/bullseye/versionsToBuild.txt
echo "20.20.0" >> platforms/nodejs/versions/bookworm/versionsToBuild.txt

# 3. Update constants.yml
# node20Version: 20.20.0

# 4. Update Test Version Constants
# In src/BuildScriptGenerator/Node/NodeVersions.cs, change Node20Version to "20.20.0"
```

#### PHP Example
```bash
# 1. Check constants.yml: php84osFlavors: bullseye,bookworm
# 2. Get SHA256 and GPG keys for PHP 8.4.11
# 3. Update versionsToBuild.txt files
echo "8.4.11, <sha256>, <gpg_keys>," >> platforms/php/versions/bullseye/versionsToBuild.txt
echo "8.4.11, <sha256>, <gpg_keys>," >> platforms/php/versions/bookworm/versionsToBuild.txt

# 4. Update constants.yml
# php84Version: 8.4.11
# php84Version_SHA: <sha256>

# 5. Update Test Version Constants
# In src/BuildScriptGenerator/PhpVersions.cs, change Php84Version to "8.4.11", update Php84TarSha256 to "<sha256>" and Php84GpgKeys to "<gpg_keys>"
```

#### Python Example
```bash
# 1. Check constants.yml: python312osFlavors: bullseye,bookworm
# 2. Get SHA256 for Python 3.12.8
# 3. Update versionsToBuild.txt files
echo "3.12.8, <sha256>" >> platforms/python/versions/bullseye/versionsToBuild.txt
echo "3.12.8, <sha256>" >> platforms/python/versions/bookworm/versionsToBuild.txt

# 4. Update constants.yml
# python312Version: 3.12.8

# 5. Update Test Version Constants
# In src/BuildScriptGenerator/PythonVersions.cs, change Python312Version to "3.12.8"
```

#### .NET Example
```bash
# 1. Check constants.yml: dotnet90osFlavors: bookworm
# 2. Get SHA256 for .NET SDK 9.0.100 (NOT runtime version)
# 3. Update versionsToBuild.txt files with SDK VERSION
echo "9.0.100, <sdk_sha256>" >> platforms/dotnet/versions/bookworm/versionsToBuild.txt

# 4. Update constants.yml with BOTH RUNTIME COMPONENTS
# NET_CORE_APP_90: 9.0.7                    # .NET Core runtime version
# NET_CORE_APP_90_SHA: <netcore_sha256>     # .NET Core runtime SHA
# ASPNET_CORE_APP_90: 9.0.7                 # ASP.NET Core runtime version (may differ)
# ASPNET_CORE_APP_90_SHA: <aspnet_sha256>   # ASP.NET Core runtime SHA (different from .NET Core)

# 5. Update Test Version Constants with SEPARATE VERSIONS
# DotNetCoreRunTimeVersions.cs: Add BOTH runtime versions "9.0.7" for NET_CORE_APP and ASPNET_CORE_APP
# Also update their corresponding SHA256 hashes in the same file
# DotNetCoreSdkVersions.cs: Add SDK version "9.0.100"
```

### Adding Support for New OS Flavor
```bash
# 1. Create new version directory
mkdir -p platforms/nodejs/versions/noble

# 2. Create versionsToBuild.txt with supported versions
cp platforms/nodejs/versions/bookworm/versionsToBuild.txt platforms/nodejs/versions/noble/

# 3. Update constants.yml osFlavors mapping
# node20osFlavors: bullseye,bookworm,noble
```

## Troubleshooting

### Common Issues
1. **Format Mismatch**: Each stack expects a specific format in versionsToBuild.txt
2. **Missing Hashes**: PHP, DotNet require SHA256 hashes
3. **GPG Key Issues**: PHP, Python builds may fail with invalid or missing GPG keys

### Verification Steps
1. **File Format**: Ensure versionsToBuild.txt follows the exact format for each stack
2. **Hash Validation**: Verify SHA256 hashes match official sources
3. **Build Logs**: Check build output for verification failures
4. **Storage Account**: Confirm artifacts are published to expected storage locations

## Related Documentation
- Platform-specific build scripts in `platforms/{stack}/`
- Runtime Dockerfile configurations in `images/runtime/`

This workflow ensures consistent, verified, and reliable stack updates across the entire Oryx build system.
