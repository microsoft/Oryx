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
    [Command("buildpack-build", Description = "Builds an app in the current working directory " +
        "(for use in a Buildpack).")]
    internal class BuildpackBuildCommand : BuildCommand
    {
        [Option("--layers-dir <dir>", CommandOptionType.SingleValue, Description = "Layers directory path.")]
        public string LayersDir { get; set; }

        [Option("--platform-dir <dir>", CommandOptionType.SingleValue, Description = "Platform directory path.")]
        public string PlatformDir { get; set; }

        [Option("--plan-path <path>", CommandOptionType.SingleValue, Description = "Build plan TOML path.")]
        public string PlanPath { get; set; }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackBuildCommand>>();

            if (!Directory.Exists(LayersDir))
            {
                logger.LogError("Could not find layers directory {layersDir}", LayersDir);
                console.Error.WriteLine($"Error: Could not find layers directory '{LayersDir}'.");
                result = false;
            }

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

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            console.WriteLine("# Stdin:");
            console.WriteLine(console.In.ReadToEnd());
            console.WriteLine("# End Stdin");

            return base.Execute(serviceProvider, console);
        }
    }
}
