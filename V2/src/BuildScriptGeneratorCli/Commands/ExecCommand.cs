// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class ExecCommand : CommandBase
    {
        public const string Name = "exec";
        public const string Description =
            "Execute an arbitrary command in the default shell, in an environment " +
            "with the best-matching platform tool versions.";

        public ExecCommand()
        {
        }

        public ExecCommand(ExecCommandProperty input)
        {
            this.SourceDir = input.SourceDir;
            this.Command = input.Command;
            this.DebugMode = input.DebugMode;
            this.LogFilePath = input.LogPath;
        }

        public string SourceDir { get; set; }

        public string Command { get; set; }

        public static Command Export(IConsole console)
        {
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);
            var execSourceDirOption = new Option<string>(
                aliases: OptionArgumentTemplates.Source,
                description: OptionArgumentTemplates.ExecSourceDescription);
            var commandArgument = new Argument<string>("command", "The command to execute in an app-specific environment.");

            var command = new Command(
                name: Name,
                description: Description)
            {
                logOption,
                debugOption,
                execSourceDirOption,
                commandArgument,
            };

            command.SetHandler(
                (prop) =>
                {
                    var execCommand = new ExecCommand(prop);
                    return Task.FromResult(execCommand.OnExecute(console));
                },
                new ExecCommandBinder(
                    execSourceDir: execSourceDirOption,
                    command: commandArgument,
                    logPath: logOption,
                    debugMode: debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ExecCommand>>();
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
            var env = serviceProvider.GetRequiredService<IEnvironment>();
            var opts = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            var beginningOutputLog = GetBeginningCommandOutputLog();
            console.WriteLine(beginningOutputLog);

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                logger.LogDebug("Command is empty; exiting");
                return ProcessConstants.ExitSuccess;
            }

            var shellPath = env.GetEnvironmentVariable("BASH") ?? FilePaths.Bash;
            var context = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);
            var detector = serviceProvider.GetRequiredService<DefaultPlatformsInformationProvider>();
            var platformInfos = detector.GetPlatformsInfo(context);
            if (!platformInfos.Any())
            {
                return ProcessConstants.ExitFailure;
            }

            int exitCode;
            using (var timedEvent = telemetryClient.LogTimedEvent("ExecCommand"))
            {
                // Build envelope script
                var scriptBuilder = new ShellScriptBuilder("\n")
                    .AddShebang(shellPath)
                    .AddCommand("set -e");

                var detectedPlatforms = platformInfos.Select(pi => pi.DetectorResult);
                var installationScriptProvider = serviceProvider.GetRequiredService<PlatformsInstallationScriptProvider>();
                var installationScript = installationScriptProvider.GetBashScriptSnippet(
                    context,
                    detectedPlatforms);
                if (!string.IsNullOrEmpty(installationScript))
                {
                    scriptBuilder.AddCommand(installationScript);
                }

                scriptBuilder.Source(
                    $"{FilePaths.Benv} " +
                    $"{string.Join(" ", detectedPlatforms.Select(p => $"{p.Platform}={p.PlatformVersion}"))}");

                scriptBuilder
                    .AddCommand("echo Executing supplied command...")
                    .AddCommand(this.Command);

                // Create temporary file to store script
                // Get the path where the generated script should be written into.
                var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
                var tempScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "execCommand.sh");
                var script = scriptBuilder.ToString();
                File.WriteAllText(tempScriptPath, script);
                console.WriteLine("Finished generating script.");

                timedEvent.AddProperty(nameof(tempScriptPath), tempScriptPath);

                if (this.DebugMode)
                {
                    console.WriteLine($"Temporary script @ {tempScriptPath}:");
                    console.WriteLine("---");
                    console.WriteLine(script);
                    console.WriteLine("---");
                }

                console.WriteLine(string.Empty);
                console.WriteLine("Executing generated script...");
                console.WriteLine(string.Empty);

                exitCode = ProcessHelper.RunProcess(
                    shellPath,
                    new[] { tempScriptPath },
                    opts.SourceDir,
                    (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            console.WriteLine(args.Data);
                        }
                    },
                    (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            console.WriteErrorLine(args.Data);
                        }
                    },
                    waitTimeForExit: null);
                timedEvent.AddProperty("exitCode", exitCode.ToString());
            }

            return exitCode;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(options, sourceDir: this.SourceDir);
        }

        internal override IServiceProvider TryGetServiceProvider(IConsole console)
        {
            // Gather all the values supplied by the user in command line
            this.SourceDir = string.IsNullOrEmpty(this.SourceDir) ?
                Directory.GetCurrentDirectory() : Path.GetFullPath(this.SourceDir);

            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var config = new ConfigurationBuilder()
                .AddIniFile(Path.Combine(this.SourceDir, Constants.BuildEnvironmentFileName), optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Override the GetServiceProvider() call in CommandBase to pass the IConsole instance to
            // ServiceProviderBuilder and allow for writing to the console if needed during this command.
            var serviceProviderBuilder = new ServiceProviderBuilder(this.LogFilePath, console)
                .ConfigureServices(services =>
                {
                    // Configure Options related services
                    // We first add IConfiguration to DI so that option services like
                    // `DotNetCoreScriptGeneratorOptionsSetup` services can get it through DI and read from the config
                    // and set the options.
                    services
                        .AddSingleton<IConfiguration>(config)
                        .AddOptionsServices()
                        .Configure<BuildScriptGeneratorOptions>(options =>
                        {
                            // These values are not retrieved through the 'config' api since we do not expect
                            // them to be provided by an end user.
                            options.SourceDir = this.SourceDir;
                            options.DebianFlavor = this.ResolveOsType(options, console);
                            options.ImageType = this.ResolveImageType(options, console);
                        });
                });

            return serviceProviderBuilder.Build();
        }
    }
}
