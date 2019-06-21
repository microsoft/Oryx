// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Execute an arbitrary command in the default shell, in an environment " +
        "with the best-matching platform tool versions.")]
    internal class ExecCommand : CommandBase
    {
        public const string Name = "exec";

        public const string DefaultBashPath = "/bin/bash";

        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        [Argument(1, Description = "The command to execute in an app-specific environment.")]
        public string Command { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();
            var env = serviceProvider.GetRequiredService<IEnvironment>();
            var generator = serviceProvider.GetRequiredService<IBuildScriptGenerator>();

            var shellPath = env.GetEnvironmentVariable("BASH") ?? DefaultBashPath;
            logger.LogInformation("Using shell {shell}", shellPath);

            var ctx = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);
            ctx.DisableMultiPlatformBuild = false;
            var tools = generator.GetRequiredToolVersions(ctx);

            int exitCode;
            using (var timedEvent = logger.LogTimedEvent("ExecCommand"))
            {
                var cmd = $"benv {StringExtensions.JoinKeyValuePairs(tools)} {Command}";

                logger.LogInformation("Running {shell} -c {cmd}", shellPath, cmd);
                if (DebugMode)
                {
                    console.WriteLine($"> {shellPath} -c '{cmd}'");
                }

                exitCode = serviceProvider.GetRequiredService<IScriptExecutor>().ExecuteScript(
                    shellPath,
                    new[] { "-c", cmd },
                    SourceDir,
                    (sender, args) => console.WriteLine(args.Data),
                    (sender, args) => console.Error.WriteLine(args.Data));

                timedEvent.AddProperty("exitCode", exitCode.ToString());
            }

            return exitCode;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            if (!Directory.Exists(options.SourceDir))
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

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                SourceDir,
                null,
                null,
                null,
                null,
                scriptOnly: false,
                null);
        }
    }
}
