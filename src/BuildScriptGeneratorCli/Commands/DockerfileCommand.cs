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
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Common;
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
            "--output",
            CommandOptionType.SingleValue,
            Description = "The path that the dockerfile will be written to. " +
                          "If not specified, the contents of the dockerfile will be written to STDOUT.")]
        public string OutputPath { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(SourceDir, loggerFactory);
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

            if (string.IsNullOrEmpty(OutputPath))
            {
                console.WriteLine(dockerfile);
            }
            else
            {
                OutputPath.SafeWriteAllText(dockerfile);
                OutputPath = Path.GetFullPath(OutputPath).TrimEnd('/').TrimEnd('\\');
                console.WriteLine($"Dockerfile written to '{OutputPath}'.");
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            SourceDir = string.IsNullOrEmpty(SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);
            if (!Directory.Exists(SourceDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{SourceDir}'.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(PlatformName) && !string.IsNullOrEmpty(PlatformVersion))
            {
                console.WriteErrorLine("Cannot use language version without specifying language name also.");
                return false;
            }

            return true;
        }

        internal override IServiceProvider GetServiceProvider(IConsole console)
        {
            // Gather all the values supplied by the user in command line
            SourceDir = string.IsNullOrEmpty(SourceDir) ?
                Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);

            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var config = new ConfigurationBuilder()
                .AddIniFile(Path.Combine(SourceDir, Constants.BuildEnvironmentFileName), optional: true)
                .AddEnvironmentVariables()
                .Add(GetCommandLineConfigSource())
                .Build();

            // Override the GetServiceProvider() call in CommandBase to pass the IConsole instance to
            // ServiceProviderBuilder and allow for writing to the console if needed during this command.
            var serviceProviderBuilder = new ServiceProviderBuilder(LogFilePath, console)
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
                            options.SourceDir = SourceDir;
                            options.ScriptOnly = false;
                        });
                });

            return serviceProviderBuilder.Build();
        }

        private CustomConfigurationSource GetCommandLineConfigSource()
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            commandLineConfigSource.Set(SettingsKeys.PlatformName, PlatformName);
            commandLineConfigSource.Set(SettingsKeys.PlatformVersion, PlatformVersion);

            // Set the platform key and version in the format that they are represented in other sources
            // (like environment variables and build.env file).
            // This is so that this enables Configuration api to apply the hierarchical config.
            // Example: "--platform python --platform-version 3.6" will win over "PYTHON_VERSION=3.7"
            // in environment variable
            SetPlatformVersion(PlatformName, PlatformVersion);

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