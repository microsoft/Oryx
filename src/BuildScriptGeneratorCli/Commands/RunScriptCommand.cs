// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("run-script", Description = "Generate startup script.")]
    internal class RunScriptCommand : BaseCommand
    {
        [Option(
            "--platform",
            CommandOptionType.SingleValue,
            Description = "The name of the programming platform, e.g. 'nodejs'.",
            ValueName = "Platform name")]
        public string PlatformName { get; set; }

        [Option(
            "--platform-version|-v",
            CommandOptionType.SingleValue,
            Description = "The version of the platform to run the application on, e.g. '10' for nodejs.",
            ValueName = "PlatformVersion")]
        public string PlatformVersion { get; set; }

        [Option(
            "--appPath",
            CommandOptionType.SingleValue,
            Description = "The path to the application folder, e.g. '/home/site/wwwroot/'.")]
        public string AppPath { get; set; } = ".";

        [Option(
            "--bindPort",
            CommandOptionType.SingleValue,
            Description = "[Optional] Port where the application will bind to.")]
        public int BindPort { get; set; } = 8080;

        [Option(
            "--userStartupCommand",
            CommandOptionType.SingleValue,
            Description = "[Optional] Command that will be executed to start the application up.")]
        public string UserStartupCommand { get; set; }

        [Option(
            "--defaultApp",
            CommandOptionType.SingleValue,
            Description = "[Optional] Path to a default file that will be executed if the entrypoint" +
            " is not found. Ex: '/opt/startup/default-static-site.js'.")]
        public string DefaultApp { get; set; }

        [Option(
            "--serverCmd",
            CommandOptionType.SingleValue,
            Description = "[Optional] Command to start the server, if different than the default," +
            " e.g. 'pm2 start --no-daemon'.")]
        public string ServerCmd { get; set; }

        [Option(
            "--debugMode",
            CommandOptionType.SingleValue,
            Description = "[Optional] Run the app in debug mode.")]
        public DebuggingMode DebugMode { get; set; }

        [Option(
            "--debugPort",
            CommandOptionType.SingleValue,
            Description = "[Optional] Debug port.")]
        public int DebugPort { get; set; }

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "[Optional] Path to the script to be generated.")]
        public string OutputPath { get; set; } = "run.sh";

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            if (string.IsNullOrWhiteSpace(PlatformName))
            {
                console.WriteLine("Platform name is required.");
                return ProcessConstants.ExitFailure;
            }

            AppPath = string.IsNullOrWhiteSpace(AppPath) ? "." : AppPath;
            string appFullPath = Path.GetFullPath(AppPath);
            string defaultAppFullPath = string.IsNullOrWhiteSpace(DefaultApp) ? null : Path.GetFullPath(DefaultApp);
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(appFullPath, loggerFactory);
            var options = new RunScriptGeneratorOptions
            {
                CustomServerCommand = ServerCmd,
                DebuggingMode = DebugMode,
                DebugPort = DebugPort,
                DefaultAppPath = defaultAppFullPath,
                SourceRepo = sourceRepo,
                UserStartupCommand = UserStartupCommand,
                PlatformVersion = PlatformVersion,
                BindPort = BindPort,
            };
            var runScriptGenerator = serviceProvider.GetRequiredService<IRunScriptGenerator>();
            var script = runScriptGenerator.GenerateBashScript(PlatformName, options);
            if (string.IsNullOrEmpty(script))
            {
                console.WriteLine("Couldn't generate startup script.");
                return ProcessConstants.ExitFailure;
            }
            else
            {
                File.WriteAllText(OutputPath, script);
                console.WriteLine($"Script written to '{OutputPath}'");
            }

            // Try making the script executable
            ProcessHelper.TrySetExecutableMode(OutputPath);

            return ProcessConstants.ExitSuccess;
        }
    }
}