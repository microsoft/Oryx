// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("buildpack-detect", Description = "Determines whether Oryx can be applied as a buildpack to " +
        "an app in the current working directory.")]
    internal class BuildpackDetectCommand : CommandBase
    {
        // CodeDetectFail @ https://github.com/buildpack/lifecycle/blob/master/detector.go
        public const int DetectorFailCode = 100;

        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        [Option("--platform-dir <dir>", CommandOptionType.SingleValue, Description = "Platform directory path.")]
        public string PlatformDir { get; set; }

        [Option("--plan-path <path>", CommandOptionType.SingleValue, Description = "Build plan TOML path.")]
        public string PlanPath { get; set; }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var logger = serviceProvider.GetService<ILogger<BuildpackDetectCommand>>();
            
            if (!Directory.Exists(SourceDir))
            {
                logger.LogError("Could not find the source directory {srcDir}", SourceDir);
                console.Error.WriteLine($"Error: Could not find the source directory '{SourceDir}'.");
                result = false;
            }

            if (!File.Exists(PlanPath))
            {
                logger?.LogError("Could not find build plan file {planPath}", PlanPath);
                console.Error.WriteLine($"Error: Could not find build plan file '{PlanPath}'.");
                result = false;
            }

            if (!Directory.Exists(PlatformDir))
            {
                logger?.LogError("Could not find platform directory {platformDir}", PlatformDir);
                console.Error.WriteLine($"Error: Could not find platform directory '{PlatformDir}'.");
                result = false;
            }

            return result;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options, SourceDir, null, null, null, null, scriptOnly: false, null);
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
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

            return DetectorFailCode;
        }
    }
}
