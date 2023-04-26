// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class Program
    {
        public const string GitCommit = "GitCommit";
        public const string ReleaseTagName = "RELEASE_TAG_NAME";

        internal static async Task<int> Main(string[] args)
        {
            var console = new SystemConsole();
            var rootCommand = new RootCommand();
            rootCommand.Name = "oryx";
            rootCommand.Description = "Generates and runs build scripts for multiple platforms.";

            var infoOption = new Option<bool>(aliases: new[] { "-i", "--info" }, "Print more detailed version information.");
            rootCommand.AddCommand(BuildCommand.Export(console));
            rootCommand.AddCommand(BuildScriptCommand.Export(console));
            rootCommand.AddCommand(BuildpackBuildCommand.Export(console));
            rootCommand.AddCommand(BuildpackDetectCommand.Export(console));
            rootCommand.AddCommand(DetectCommand.Export(console));
            rootCommand.AddCommand(DockerfileCommand.Export(console));
            rootCommand.AddCommand(ExecCommand.Export(console));
            rootCommand.AddCommand(PlatformsCommand.Export(console));
            rootCommand.AddCommand(PrepareEnvironmentCommand.Export(console));
            rootCommand.AddCommand(RunScriptCommand.Export(console));
            rootCommand.AddCommand(TelemetryCommand.Export(console));
            rootCommand.AddOption(infoOption);

            rootCommand.SetHandler(
                (infoSetter) =>
            {
                var returnCode = OnExecute(console, infoSetter);
                return Task.FromResult(returnCode);
            },
                infoOption);

            // rootCommand.AddGlobalOption(versionOption);
            return await rootCommand.InvokeAsync(args, console);
        }

        internal static string GetVersion()
        {
            var informationalVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersion != null)
            {
                return informationalVersion.InformationalVersion;
            }

            return null;
        }

        internal static string GetMetadataValue(string name)
        {
            var commitMetadata = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Where(attr => attr.Key.Equals(name))
                    .FirstOrDefault();
            if (commitMetadata != null)
            {
                return commitMetadata.Value;
            }

            return null;
        }

        internal static int OnExecute(IConsole console, bool setInfo)
        {
            if (setInfo)
            {
                var version = GetVersion();
                var commit = GetMetadataValue(GitCommit);
                var releaseTagName = GetMetadataValue(ReleaseTagName);
                console.WriteLine($"Version: {version}, Commit: {commit}, ReleaseTagName: {releaseTagName}");
            }

            return ProcessConstants.ExitSuccess;
        }
    }
}