// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Generates a dockerfile to build and run an app.")]
    internal class DockerfileCommand : CommandBase
    {
        public const string Name = "dockerfile";

        [Argument(0, Description = "The source directory. If no value is provided, the current directory is used.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option(
            OptionTemplates.Platform,
            CommandOptionType.SingleValue,
            Description = "The name of the programming platform used in the provided source directory.")]
        public string PlatformName { get; set; }

        [Option(
            OptionTemplates.PlatformVersion,
            CommandOptionType.SingleValue,
            Description = "The version of the programming platform used in the provided source directory.")]
        public string PlatformVersion { get; set; }

        [Option(
            OptionTemplates.RuntimePlatform,
            CommandOptionType.SingleValue,
            Description = "The runtime platform to use in the Dockerfile. If not provided, the value for --platform will be used, otherwise the value will be auto-detected.")]
        public string RuntimePlatformName { get; set; }

        [Option(
            OptionTemplates.RuntimePlatformVersion,
            CommandOptionType.SingleValue,
            Description = "The version of the runtime to use in the Dockerfile. If not provided, the value will be 'dynamic'.")]
        public string RuntimePlatformVersion { get; set; }

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "The path that the dockerfile will be written to. " +
                          "If not specified, the contents of the dockerfile will be written to STDOUT.")]
        public string OutputPath { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(this.SourceDir, loggerFactory);
            var ctx = new DockerfileContext
            {
                SourceRepo = sourceRepo,
            };

            var dockerfileGenerator = serviceProvider.GetRequiredService<IDockerfileGenerator>();
            var dockerfile = dockerfileGenerator.GenerateDockerfile(ctx);
            if (string.IsNullOrEmpty(dockerfile))
            {
                console.WriteErrorLine("Couldn't generate dockerfile.");
                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrEmpty(this.OutputPath))
            {
                console.WriteLine(dockerfile);
            }
            else
            {
                this.OutputPath.SafeWriteAllText(dockerfile);
                this.OutputPath = Path.GetFullPath(this.OutputPath).TrimEnd('/').TrimEnd('\\');
                console.WriteLine($"Dockerfile written to '{this.OutputPath}'.");
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            this.SourceDir = string.IsNullOrEmpty(this.SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(this.SourceDir);
            if (!Directory.Exists(this.SourceDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{this.SourceDir}'.");
                return false;
            }

            // Invalid to specify platform version without platform name
            if (string.IsNullOrEmpty(this.PlatformName) && !string.IsNullOrEmpty(this.PlatformVersion))
            {
                console.WriteErrorLine("Cannot use platform version without specifying platform name also.");
                return false;
            }

            return true;
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
                .Add(this.GetCommandLineConfigSource())
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
                            // These values are not retrieve through the 'config' api since we do not expect
                            // them to be provided by an end user.
                            options.SourceDir = this.SourceDir;
                            options.ScriptOnly = false;
                        });
                });

            return serviceProviderBuilder.Build();
        }

        private CustomConfigurationSource GetCommandLineConfigSource()
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            commandLineConfigSource.Set(SettingsKeys.PlatformName, this.PlatformName);
            commandLineConfigSource.Set(SettingsKeys.PlatformVersion, this.PlatformVersion);
            commandLineConfigSource.Set(SettingsKeys.RuntimePlatformName, this.RuntimePlatformName);
            commandLineConfigSource.Set(SettingsKeys.RuntimePlatformVersion, this.RuntimePlatformVersion);

            // Set the platform key and version in the format that they are represented in other sources
            // (like environment variables and build.env file).
            // This is so that this enables Configuration api to apply the hierarchical config.
            // Example: "--platform python --platform-version 3.6" will win over "PYTHON_VERSION=3.7"
            // in environment variable
            SetPlatformVersion(this.PlatformName, this.PlatformVersion);

            return commandLineConfigSource;

            void SetPlatformVersion(string platformName, string platformVersion)
            {
                platformName = platformName == "nodejs" ? "node" : platformName;
                var platformVersionKey = $"{platformName}_version".ToUpper();
                commandLineConfigSource.Set(platformVersionKey, platformVersion);
            }
        }
    }
}