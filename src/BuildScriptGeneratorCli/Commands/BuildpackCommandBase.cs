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
    internal class BuildpackCommandBase : BaseCommand
    {
        [Option("--platform-dir <dir>", CommandOptionType.SingleValue, Description = "Platform directory.")]
        public string PlatformDir { get; set; }

        [Option("--plan-path <dir>", CommandOptionType.SingleValue, Description = "Build plan path.")]
        public string PlanPath { get; set; }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackCommandBase>>();

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
    }
}
