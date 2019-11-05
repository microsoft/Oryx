// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Generates a dockerfile to build and run an app.")]
    internal class DockerfileCommand : CommandBase
    {
        public const string Name = "dockerfile";

        [Argument(0, Description = "The source directory. If no value is provided, the current directory is used.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option(
            OptionTemplates.Platform,
            CommandOptionType.SingleValue,
            Description = "The name of the programming platform used in the provided source directory.")]
        public string PlatformName { get; set; }

        [Option(
            OptionTemplates.PlatformVersion,
            CommandOptionType.SingleValue,
            Description = "The version of the programming platform used in the provided source directory.")]
        public string PlatformVersion { get; set; }

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "The path that the dockerfile will be written to. " +
                          "If not specified, the contents of the dockerfile will be written to STDOUT.")]
        public string OutputPath { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(SourceDir, loggerFactory);
            var ctx = new DockerfileContext
            {
                SourceRepo = sourceRepo,
                Platform = PlatformName,
                PlatformVersion = PlatformVersion,
            };

            var dockerfileGenerator = serviceProvider.GetRequiredService<IDockerfileGenerator>();
            var dockerfile = dockerfileGenerator.GenerateDockerfile(ctx);
            if (string.IsNullOrEmpty(dockerfile))
            {
                console.WriteErrorLine("Couldn't generate dockerfile.");
                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrEmpty(OutputPath))
            {
                console.WriteLine(dockerfile);
            }
            else
            {
                OutputPath.SafeWriteAllText(dockerfile);
                OutputPath = Path.GetFullPath(OutputPath).TrimEnd('/').TrimEnd('\\');
                console.WriteLine($"Dockerfile written to '{OutputPath}'.");
            }

            return Execute(serviceProvider, console, stdOutHandler: null, stdErrHandler: null);
        }

        // To enable unit testing
        internal int Execute(
            IServiceProvider serviceProvider,
            IConsole console,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            SourceDir = string.IsNullOrEmpty(SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);
            if (!Directory.Exists(SourceDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{SourceDir}'.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(PlatformName) && !string.IsNullOrEmpty(PlatformVersion))
            {
                console.WriteErrorLine("Cannot use language version without specifying language name also.");
                return false;
            }

            return true;
        }
    }
}