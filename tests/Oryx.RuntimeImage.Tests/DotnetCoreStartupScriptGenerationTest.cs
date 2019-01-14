// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;
using Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.RuntimeImage.Tests
{
    public class DotnetCoreStartupScriptGenerationTest
    {
        private const string DotnetCoreRuntimeImageName = "oryxdevms/dotnetcore-2.2";

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

        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;

        public DotnetCoreStartupScriptGenerationTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
        }

        [Fact]
        public void GeneratesScript_UsingDefaultOryxPublishOutputDirectory()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedWorkingDir, result.Output);
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingExplicitAssemblyName()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"Foo.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{ProjectFileWithExplicitAssemblyName}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/Foo.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedWorkingDir, result.Output);
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingExplicitOutputDirectory()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -publishedOutputPath {appOutputDir} -output {scriptLocation}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedWorkingDir, result.Output);
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_ForPorjectFile_HavingMultiplePropertyGroups()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet \"webapp.dll\"";
            var expectedWorkingDir = $"cd \"{appOutputDir}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{ProjectFileWithMultiplePropertyGroups}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedWorkingDir, result.Output);
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WithDefaultAppFilePath_IfNoProjectFileIsFound()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var defaultWebAppFile = "/tmp/defaultwebapp.dll";
            var expectedStartupCommand = $"dotnet \"{defaultWebAppFile}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo > /tmp/defaultwebapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation} -defaultAppFilePath {defaultWebAppFile}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_WithDefaultAppFilePath_IfNoPubllishOutputDirectoryFound()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var defaultWebAppFile = "/tmp/defaultwebapp.dll";
            var expectedStartupCommand = $"dotnet \"{defaultWebAppFile}\"";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}") // no publish directory
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"echo > /tmp/defaultwebapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation} -defaultAppFilePath {defaultWebAppFile}")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedStartupCommand, result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_UsingUserStartupCommand_AsItIs()
        {
            // Arrange
            var appDir = "/app";
            var appOutputDir = $"{appDir}/{DotnetCoreConstants.OryxOutputPublishDirectory}";
            var expectedStartupCommand = $"dotnet foo.dll";
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}")
                .AddCommand($"echo '{RegularProjectFileContent}' > {appDir}/webapp.csproj")
                .AddCommand($"mkdir -p {appOutputDir}")
                .AddCommand($"echo > {appOutputDir}/webapp.dll")
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation} -userStartupCommand \"{expectedStartupCommand}\"")
                .AddCommand($"cat {scriptLocation}")
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
                    Assert.Contains(expectedStartupCommand, result.Output);
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
            var scriptLocation = "/tmp/run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir -p {appDir}") // no .csproj file
                .AddCommand($"oryx -sourcePath {appDir} -output {scriptLocation} -defaultAppFilePath /tmp/doesnotexist.dll")
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

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
