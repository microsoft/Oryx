// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class CapabilitiesCommand
    {
        public const string Name = "capabilities";
        public const string Description = "Show machine-readable Oryx capabilities.";

        public static Command Export(IConsole console)
        {
            var formatOption = new Option<string>(
                "--format",
                "Output format. The supported value is 'json'.")
            {
                IsRequired = true,
            };
            var command = new Command(Name, Description);
            command.AddOption(formatOption);
            command.SetHandler(
                format => Task.FromResult(OnExecute(console, format)),
                formatOption);
            return command;
        }

        internal static int OnExecute(IConsole console, string format)
        {
            if (!string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                console.WriteErrorLine("The capabilities command supports only '--format json'.");
                return ProcessConstants.ExitFailure;
            }

            console.WriteLine(
                $"{{\"schemaVersion\":{DeploymentProgressConstants.SchemaVersion}," +
                $"\"capabilities\":[\"{DeploymentProgressConstants.Capability}\"]}}");
            return ProcessConstants.ExitSuccess;
        }
    }
}
