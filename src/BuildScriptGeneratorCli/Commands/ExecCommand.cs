// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(ExecCommand.Name, Description = "Execute an arbitrary command in an environment with the best-matching " +
        "platform tool versions.")]
    internal class ExecCommand : CommandBase
    {
        public const string Name = "exec";

        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        [Argument(1, Description = "The command to execute in an app-specific environment.")]
        public string Command { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();
            Console.WriteLine($"Running {Command}");
            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();

            if (!Directory.Exists(SourceDir))
            {
                logger.LogError("Could not find the source directory {srcDir}", SourceDir);
                console.Error.WriteLine($"Error: Could not find the source directory '{SourceDir}'.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Command))
            {
                console.Error.WriteLine("Error: A command is required.");
                return false;
            }

            return true;
        }
    }
}
