// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class RunScriptCommand : CommandBase
    {
        public const string Name = "run-script";
        public const string Description = "Generate startup script for an app.";

        public RunScriptCommand()
        {
        }

        public RunScriptCommand(RunScriptCommandProperty input)
        {
            this.AppDir = input.AppDir;
            this.PlatformName = input.PlatformName;
            this.PlatformVersion = input.PlatformVersion;
            this.OutputPath = input.Output;
            this.RemainingArgs = input.RemainingArgs;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public string AppDir { get; set; } = ".";

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public string OutputPath { get; set; }

        public string[] RemainingArgs { get; }

        public static Command Export(IConsole console)
        {
            var appDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.AppDir,
                description: OptionArgumentTemplates.AppDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var platformNameOption = new Option<string>(OptionArgumentTemplates.Platform, OptionArgumentTemplates.RunScriptPlatformDescription);
            var platformVersionOption = new Option<string>(OptionArgumentTemplates.PlatformVersion, OptionArgumentTemplates.RunScriptPlatformVersionDescription);
            var outputOption = new Option<string>(OptionArgumentTemplates.Output, OptionArgumentTemplates.RunScriptOutputDescription);
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);

            var command = new Command(Name, Description)
            {
                appDirArgument,
                platformNameOption,
                platformVersionOption,
                outputOption,
                logOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var runScriptCommand = new RunScriptCommand(prop);
                    return Task.FromResult(runScriptCommand.OnExecute(console));
                },
                new RunScriptCommandBinder(
                    appDir: appDirArgument,
                    platformName: platformNameOption,
                    platformVersion: platformVersionOption,
                    output: outputOption,
                    logPath: logOption,
                    debugMode: debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(this.AppDir, loggerFactory);

            var ctx = new RunScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
                Platform = this.PlatformName,
                PlatformVersion = this.PlatformVersion,
                PassThruArguments = this.RemainingArgs,
            };

            var runScriptGenerator = serviceProvider.GetRequiredService<IRunScriptGenerator>();
            var script = runScriptGenerator.GenerateBashScript(ctx);
            if (string.IsNullOrEmpty(script))
            {
                console.WriteErrorLine("Couldn't generate startup script.");
                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrWhiteSpace(this.OutputPath))
            {
                console.WriteLine(script);
            }
            else
            {
                File.WriteAllText(this.OutputPath, script);
                console.WriteLine($"Script written to '{this.OutputPath}'");

                // Try making the script executable
                ProcessHelper.TrySetExecutableMode(this.OutputPath);
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            this.AppDir = string.IsNullOrEmpty(this.AppDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(this.AppDir);
            if (!Directory.Exists(this.AppDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{this.AppDir}'.");
                return false;
            }

            return true;
        }
    }
}