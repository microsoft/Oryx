// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;

// using Microsoft.VisualBasic;
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class Program
    {
        public const string GitCommit = "GitCommit";
        public const string ReleaseTagName = "RELEASE_TAG_NAME";

        public bool Version { get; set; }

        internal static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.Name = "oryx";
            rootCommand.Description = "Generates and runs build scripts for multiple platforms.";

            // var versionOption = new Option<bool>(aliases: new[] { "-v", "--version" }, "Print version information.");
            rootCommand.AddCommand(BuildCommand.Export());
            rootCommand.AddCommand(PlatformsCommand.Export());
            rootCommand.AddCommand(BuildScriptCommand.Export());
            rootCommand.AddCommand(RunScriptCommand.Export());
            rootCommand.AddCommand(ExecCommand.Export());
            rootCommand.AddCommand(DetectCommand.Export());
            rootCommand.AddCommand(BuildpackDetectCommand.Export());
            rootCommand.AddCommand(BuildpackBuildCommand.Export());
            rootCommand.AddCommand(DockerfileCommand.Export());
            rootCommand.AddCommand(PrepareEnvironmentCommand.Export());
            rootCommand.AddCommand(TelemetryCommand.Export());

            // rootCommand.AddGlobalOption(versionOption);
            return await rootCommand.InvokeAsync(args);
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

        internal int OnExecute()
        {
            if (this.Version)
            {
                var version = GetVersion();
                var commit = GetMetadataValue(GitCommit);
                var releaseTagName = GetMetadataValue(ReleaseTagName);
                Console.WriteLine($"Version: {version}, Commit: {commit}, ReleaseTagName: {releaseTagName}");

                return ProcessConstants.ExitSuccess;
            }

            return ProcessConstants.ExitSuccess;
        }
    }
}