// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    [Command("install-runtime", Description = "Install the required runtime components for a platform.")]
    class InstallRunTimeCommand : CommandBase
    {
        [Option(
            "-p|--platform <name>",
            CommandOptionType.SingleValue,
            Description = "The name of the platform for which the runtime components should be installed.")]
        public string Platform { get; set; }

        [Option(
            "--platform-version <version>",
            CommandOptionType.SingleValue,
            Description = "The version of the platform for which the runtime components should be installed.")]
        public string PlatformVersion { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new RunTimeInstallationScriptGenerator(serviceProvider, console);

            if (!scriptGenerator.TryGenerateScript(out var generatedScript))
            {
                return ProcessConstants.ExitFailure;
            }

            console.WriteLine(generatedScript);

            return ProcessConstants.ExitSuccess;
        }
    }
}
