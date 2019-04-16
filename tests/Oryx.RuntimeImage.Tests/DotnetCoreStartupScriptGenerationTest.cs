// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class DotnetCoreStartupScriptGenerationTest : TestBase
    {
        private const string DotnetCoreRuntimeImageName = "oryxdevms/dotnetcore-2.2";
        private const string ScriptLocation = "./run.sh";

        private const string RegularProjectFileContent =
            @"<Project Sdk=""Microsoft.NET.Sdk.Web"">" +
            "<PropertyGroup><TargetFramework>netcoreapp2.1</TargetFramework></PropertyGroup>" +
            @"<ItemGroup><PackageReference Include=""Microsoft.AspNetCore.App"" /></ItemGroup>" +
            "</Project>";

        private const string ProjectFileWithExplicitAssemblyName =
            @"<Project Sdk=""Microsoft.NET.Sdk.Web"">" +
            "<PropertyGroup><TargetFramework>netcoreapp2.1</TargetFramework><AssemblyName>Foo</AssemblyName></PropertyGroup>" +
            @"<ItemGroup><PackageReference Include=""Microsoft.AspNetCore.App"" /></ItemGroup></Project>";

        private const string ProjectFileWithMultiplePropertyGroups =
            @"<Project Sdk=""Microsoft.NET.Sdk.Web"">" +
            "<PropertyGroup><Foo>Bar</Foo></PropertyGroup>" +
            "<PropertyGroup><TargetFramework>netcoreapp2.1</TargetFramework></PropertyGroup>" +
            @"<ItemGroup><PackageReference Include=""Microsoft.AspNetCore.App"" /></ItemGroup>" +
            "</Project>";

        public DotnetCoreStartupScriptGenerationTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GeneratesScript_UsingProjectFileName()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet \"shoppingapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/shoppingapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/shoppingapp.dll")
                .AddCommand($"oryx -appPath {appOutputDir} -sourcePath {appDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingStartupFileName_FromBuildManifest_NotUsingSourcePath()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet \"different.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/shoppingapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo startupFileName=\\\"different.dll\\\" > {appOutputDir}/oryx-manifest.toml")
                .AddCommand($"echo > {appOutputDir}/different.dll")
                // NOTE: Do not specify source path argument
                .AddCommand($"oryx -appPath {appOutputDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingSourcePath_IfStartupFileNameIsEmpty_FromSourcePath()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet \"shoppingapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/shoppingapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo startupFileName=\\\"\\\" > {appOutputDir}/oryx-manifest.toml")
                .AddCommand($"echo > {appOutputDir}/shoppingapp.dll")
                .AddCommand($"oryx -appPath {appOutputDir} -sourcePath {appDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WithDefaultAppFilePath_IfStartupFileFromBuildManifest_DoesNotExist()
        {
            // Arrange
            var appDir = "/app";
            var outputDir = "/app/output";
            var defaultWebAppFile = "/tmp/defaultwebapp.dll";
            var expectedStartupCommand = $"dotnet \"{defaultWebAppFile}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/shoppingapp.csproj")
                .AddCommand($"mkdir -p {outputDir}")
                .AddCommand($"echo startupFileName=\\\"doesnotexist.dll\\\" > {outputDir}/oryx-manifest.toml")
                .AddCommand($"echo > /tmp/defaultwebapp.dll")
                .AddCommand($"oryx -appPath {outputDir} -defaultAppFilePath {defaultWebAppFile}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingExplicitAssemblyName()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet \"Foo.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{ProjectFileWithExplicitAssemblyName}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/Foo.dll")
                .AddCommand($"oryx -appPath {appOutputDir} -sourcePath {appDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        // AppService scenario
        [Fact]
        public void GeneratesScript_UsingExplicitOutputDirectory_AndOutputDirectoryIsCurrentDirectory()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/tmp/foo";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                // NOTE: Make sure the current directory is the output directory itself and do NOT supply the 
                // 'appPath' argument.
                .AddCommand($"cd {appOutputDir}")
                .AddCommand($"oryx -sourcePath {appDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_ForPorjectFile_HavingMultiplePropertyGroups()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{ProjectFileWithMultiplePropertyGroups}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand($"oryx -appPath {appOutputDir} -sourcePath {appDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WithDefaultAppFilePath_IfNoProjectFileIsFound()
        {
            // Arrange
            var appDir = "/app";
            var outputDir = "/app/output";
            var defaultWebAppFile = "/tmp/defaultwebapp.dll";
            var expectedStartupCommand = $"dotnet \"{defaultWebAppFile}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}") //NOTE: no project file
                .AddCommand($"mkdir -p {outputDir}")
                .AddCommand($"echo > /tmp/defaultwebapp.dll")
                .AddCommand($"oryx -appPath {outputDir} -sourcePath {appDir} -defaultAppFilePath {defaultWebAppFile}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingUserStartupCommand_AsItIs()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = "/app/output";
            var expectedStartupCommand = $"dotnet foo.dll";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand(
                $"oryx -appPath {appOutputDir} -sourcePath {appDir} -userStartupCommand \"{expectedStartupCommand}\"")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ScriptGenerationFails_IfSourcePathDoesNotExist()
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand("oryx -sourcePath /doesnotexist")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ScriptGenerationFails_IfDefaultAppFilePath_DoesNotExist()
        {
            // Arrange
            var appDir = "/app";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}") // no .csproj file
                .AddCommand($"oryx -sourcePath {appDir} -defaultAppFilePath /tmp/doesnotexist.dll")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingProjectEnvironmentVariable()
        {
            // Arrange
            var repoDir = "/repo";
            var webApp1Dir = $"{repoDir}/src/Apps/WebApp1";
            var webApp2Dir = $"{repoDir}/src/Apps/WebApp2";
            var webApp1OutputDir = $"{webApp1Dir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"webapp1.dll\"";
            var expectedWorkingDir = $"cd \"{webApp1OutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"export PROJECT=src/Apps/WebApp1/webapp1.csproj")
                .AddCommand($"mkdir -p {webApp1Dir}")
                .AddCommand($"mkdir -p {webApp2Dir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {webApp1Dir}/webapp1.csproj")
                .AddCommand($"echo '{RegularProjectFileContent}' > {webApp2Dir}/webapp2.csproj")
                .AddCommand($"mkdir -p {webApp1OutputDir}")
                .AddCommand($"echo > {webApp1OutputDir}/webapp1.dll")
                .AddCommand($"oryx -appPath {webApp1OutputDir} -sourcePath {repoDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WhenWebApplicationProject_IsDeeplyNested()
        {
            // Arrange
            var repoDir = "/repo";
            var webAppDir = $"{repoDir}/src/Apps/WebApp";
            var webAppOutputDir = $"{webAppDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{webAppOutputDir}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {webAppDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {webAppDir}/webapp.csproj")
                .AddCommand($"mkdir -p {webAppOutputDir}")
                .AddCommand($"echo > {webAppOutputDir}/webapp.dll")
                .AddCommand($"oryx -appPath {webAppOutputDir} -sourcePath {repoDir}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedWorkingDir, result.StdOut);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WithDefaultAppFilePath_IfMultipleWebApplicationProjectsFound()
        {
            // Arrange
            var repoDir = "/repo";
            var outputDir = "/repo/output";
            var webApp1Dir = $"{repoDir}/src/Apps/WebApp1";
            var webApp2Dir = $"{repoDir}/src/Apps/WebApp2";
            var defaultWebAppFile = "/tmp/defaultwebapp.dll";
            var expectedStartupCommand = $"dotnet \"{defaultWebAppFile}\"";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {webApp1Dir}")
                .AddCommand($"mkdir -p {webApp2Dir}")
                .AddCommand($"mkdir -p {outputDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {webApp1Dir}/webapp1.csproj")
                .AddCommand($"echo '{RegularProjectFileContent}' > {webApp2Dir}/webapp2.csproj")
                .AddCommand($"echo > {defaultWebAppFile}")
                .AddCommand($"oryx -appPath {outputDir} -sourcePath {repoDir} -defaultAppFilePath {defaultWebAppFile}")
                .AddCommand($"cat {ScriptLocation}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                DotnetCoreRuntimeImageName,
                commandToExecuteOnRun: "/bin/sh",
                commandArguments: new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedStartupCommand, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratedScript_CanRunStartupScriptsFromAppRoot()
        {
            // Arrange
            const int exitCodeSentinel = 222;
            var appPath = "/tmp/app";
            var script = new ShellScriptBuilder()
                .CreateDirectory(appPath)
                .CreateFile(appPath + "/entry.sh", $"exit {exitCodeSentinel}")
                .AddCommand("oryx -userStartupCommand entry.sh -sourcePath " + appPath + " -appPath " + appPath)
                .AddCommand(". ./run.sh") // Source the default output path
                .ToString();

            // Act
            var res = _dockerCli.Run("oryxdevms/dotnetcore-2.2", "/bin/sh", new[] { "-c", script });

            // Assert
            RunAsserts(() => Assert.Equal(res.ExitCode, exitCodeSentinel), res.GetDebugInfo());
        }
    }
}
