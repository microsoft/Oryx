// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Build an app in the current working directory (for use in a Buildpack).")]
    internal class BuildpackBuildCommand : BuildCommand
    {
        public new const string Name = "buildpack-build";

        [Option("--layers-dir <dir>", CommandOptionType.SingleValue, Description = "Layers directory path.")]
        public string LayersDir { get; set; }

        [Option("--platform-dir <dir>", CommandOptionType.SingleValue, Description = "Platform directory path.")]
        public string PlatformDir { get; set; }

        [Option("--plan-path <path>", CommandOptionType.SingleValue, Description = "Build plan TOML path.")]
        public string PlanPath { get; set; }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = base.IsValidInput(serviceProvider, console);
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackBuildCommand>>();

            if (!string.IsNullOrWhiteSpace(LayersDir))
            {
                LayersDir = Path.GetFullPath(LayersDir);
                if (!Directory.Exists(LayersDir))
                {
                    logger.LogError("Could not find layers directory {layersDir}", LayersDir);
                    console.Error.WriteLine($"Error: Could not find layers directory '{LayersDir}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(PlanPath))
            {
                PlanPath = Path.GetFullPath(PlanPath);
                if (!File.Exists(PlanPath))
                {
                    logger?.LogError("Could not find build plan file {planPath}", PlanPath);
                    console.Error.WriteLine($"Error: Could not find build plan file '{PlanPath}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(PlatformDir))
            {
                PlatformDir = Path.GetFullPath(PlatformDir);
                if (!Directory.Exists(PlatformDir))
                {
                    logger?.LogError("Could not find platform directory {platformDir}", PlatformDir);
                    console.Error.WriteLine($"Error: Could not find platform directory '{PlatformDir}'.");
                    result = false;
                }
            }

            return result;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            return base.Execute(serviceProvider, console);
        }
    }
}
