// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("buildpack-detect", Description = "Determines whether Oryx can build an app in " +
        "the current working directory (for use in a Buildpack).")]
    internal class BuildpackDetectCommand : CommandBase
    {
        [Option("--platform-dir <dir>", CommandOptionType.SingleValue, Description = "Platform directory path.")]
        public string PlatformDir { get; set; }

        [Option("--plan-path <path>", CommandOptionType.SingleValue, Description = "Build plan TOML path.")]
        public string PlanPath { get; set; }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackDetectCommand>>();

            if (!File.Exists(PlanPath))
            {
                logger.LogError("Could not find build plan file {planPath}", PlanPath);
                console.Error.WriteLine($"Error: Could not find build plan file '{PlanPath}'.");
                result = false;
            }

            if (!Directory.Exists(PlatformDir))
            {
                logger.LogError("Could not find platform directory {platformDir}", PlatformDir);
                console.Error.WriteLine($"Error: Could not find platform directory '{PlatformDir}'.");
                result = false;
            }

            return result;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options, Directory.GetCurrentDirectory(), null, null, null, null, scriptOnly: false, null);
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackDetectCommand>>();
            var generator = serviceProvider.GetRequiredService<IBuildScriptGenerator>();

            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var env = serviceProvider.GetRequiredService<CliEnvironmentSettings>();
            var repo = serviceProvider.GetRequiredService<ISourceRepoProvider>().GetSourceRepo();

            var ctx = BuildScriptGenerator.CreateContext(options, env, repo);
            var compatPlats = generator.GetCompatiblePlatforms(ctx);

            if (compatPlats != null && compatPlats.Any())
            {
                console.WriteLine("# Detected platforms:");
                console.WriteLine(string.Join(' ', compatPlats.Select(pair => $"{pair.Item1.Name}=\"{pair.Item2}\"")));
                return ProcessConstants.ExitSuccess;
            }

            return 100; // CodeDetectFail in buildpack/lifecycle/detector.go
        }
    }
}
