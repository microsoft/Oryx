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

            if (!string.IsNullOrWhiteSpace(this.LayersDir))
            {
                this.LayersDir = Path.GetFullPath(this.LayersDir);
                if (!Directory.Exists(this.LayersDir))
                {
                    logger.LogError("Could not find provided layers directory.");
                    console.WriteErrorLine($"Could not find layers directory '{this.LayersDir}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.PlanPath))
            {
                this.PlanPath = Path.GetFullPath(this.PlanPath);
                if (!File.Exists(this.PlanPath))
                {
                    logger?.LogError("Could not find build plan file {planPath}", this.PlanPath);
                    console.WriteErrorLine($"Could not find build plan file '{this.PlanPath}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.PlatformDir))
            {
                this.PlatformDir = Path.GetFullPath(this.PlatformDir);
                if (!Directory.Exists(this.PlatformDir))
                {
                    logger?.LogError("Could not find platform directory {platformDir}", this.PlatformDir);
                    console.WriteErrorLine($"Could not find platform directory '{this.PlatformDir}'.");
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
