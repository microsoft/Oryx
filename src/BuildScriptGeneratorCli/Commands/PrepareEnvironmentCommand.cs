// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Prepares environment with required versions of platform sdks.")]
    internal class PrepareEnvironmentCommand : CommandBase
    {
        public const string Name = "prep-env";

        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var generator = serviceProvider.GetRequiredService<IBuildScriptGenerator>();

            var context = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);
            var compatPlats = generator.GetCompatiblePlatforms(context);

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("#!/bin/bash");
            scriptBuilder.AppendLine("set -ex");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("echo Setting up the environment...");
            scriptBuilder.AppendLine(
                "prepEnv " + string.Join(" ", compatPlats.Select(t => $"{t.Item1.Name}={t.Item2}")));
            var scriptContent = scriptBuilder.ToString();
            
            // Get the path where the generated script should be written into.
            var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
            var buildScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "prepareEnvironment.sh");

            // Write build script to selected path
            File.WriteAllText(buildScriptPath, scriptContent);

            DataReceivedEventHandler stdOutBaseHandler = (sender, args) =>
            {
                string line = args.Data;
                if (line == null)
                {
                    return;
                }

                console.WriteLine(line);
            };

            DataReceivedEventHandler stdErrBaseHandler = (sender, args) =>
            {
                string line = args.Data;
                if (line == null)
                {
                    return;
                }

                // Not using IConsole.WriteErrorLine intentionally, to keep the child's error stream intact
                console.Error.WriteLine(line);
            };

            // Run the generated script
            int exitCode;
            exitCode = serviceProvider.GetRequiredService<IScriptExecutor>().ExecuteScript(
                buildScriptPath,
                args: null,
                workingDirectory: context.SourceRepo.RootPath,
                stdOutBaseHandler,
                stdErrBaseHandler);

            if (exitCode != ProcessConstants.ExitSuccess)
            {
                return exitCode;
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var logger = serviceProvider.GetService<ILogger<BuildpackDetectCommand>>();

            // Set from ConfigureBuildScriptGeneratorOptions
            if (!Directory.Exists(options.SourceDir))
            {
                logger.LogError("Could not find the source directory {srcDir}", options.SourceDir);
                console.WriteErrorLine($"Could not find the source directory '{options.SourceDir}'.");
                result = false;
            }
            
            return result;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: SourceDir,
                destinationDir: null,
                intermediateDir: null,
                manifestDir: null,
                platform: null,
                platformVersion: null,
                scriptOnly: false,
                properties: null);
        }
    }
}
