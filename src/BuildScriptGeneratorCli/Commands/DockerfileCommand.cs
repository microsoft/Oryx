// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class DockerfileCommand : CommandBase
    {
        public const string Name = "dockerfile";
        public const string Description = "Generates a dockerfile to build and run an app.";

        private readonly string[] supportedRuntimePlatforms = { "dotnetcore", "node", "php", "python", "ruby" };

        public DockerfileCommand()
        {
        }

        public DockerfileCommand(DockerfileCommandProperty input)
        {
            this.SourceDir = input.SourceDir;
            this.BuildImage = input.BuildImage;
            this.PlatformName = input.Platform;
            this.PlatformVersion = input.PlatformVersion;
            this.RuntimePlatformName = input.RuntimePlatform;
            this.RuntimePlatformVersion = input.RuntimePlatformVersion;
            this.BindPort = input.BindPort;
            this.OutputPath = input.Output;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public string SourceDir { get; set; }

        public string BuildImage { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public string RuntimePlatformName { get; set; }

        public string RuntimePlatformVersion { get; set; }

        public string BindPort { get; set; }

        public string OutputPath { get; set; }

        public static Command Export(IConsole console)
        {
            var sourceDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.SourceDir,
                description: OptionArgumentTemplates.DockerfileSourceDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var buildImageOption = new Option<string>(
                name: OptionArgumentTemplates.DockerfileBuildImage,
                description: OptionArgumentTemplates.DockerfileBuildImageDescription);
            var platformOption = new Option<string>(OptionArgumentTemplates.Platform, OptionArgumentTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionArgumentTemplates.PlatformVersion, OptionArgumentTemplates.PlatformVersionDescription);
            var runtimePlatformOption = new Option<string>(OptionArgumentTemplates.RuntimePlatform, OptionArgumentTemplates.RuntimePlatformDescription);
            var runtimePlatformVersionOption = new Option<string>(OptionArgumentTemplates.RuntimePlatformVersion, OptionArgumentTemplates.RuntimePlatformVersionDescription);
            var bindPortOption = new Option<string>(
                name: OptionArgumentTemplates.DockerfileBindPort,
                description: OptionArgumentTemplates.DockerfileBindPortDescription);
            var outputOption = new Option<string>(
                aliases: OptionArgumentTemplates.Output,
                description: OptionArgumentTemplates.DockerfileOutputDescription);
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);

            var command = new Command(Name, Description)
            {
                sourceDirArgument,
                buildImageOption,
                platformOption,
                platformVersionOption,
                runtimePlatformOption,
                runtimePlatformVersionOption,
                bindPortOption,
                outputOption,
                logOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var dockerfileCommand = new DockerfileCommand(prop);
                    return Task.FromResult(dockerfileCommand.OnExecute(console));
                },
                new DockerfileCommandBinder(
                    sourceDir: sourceDirArgument,
                    buildImage: buildImageOption,
                    platform: platformOption,
                    platformVersion: platformVersionOption,
                    runtimePlatform: runtimePlatformOption,
                    runtimePlatformVersion: runtimePlatformVersionOption,
                    bindPort: bindPortOption,
                    output: outputOption,
                    logPath: logOption,
                    debugMode: debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            int exitCode;
            var logger = serviceProvider.GetRequiredService<ILogger<DockerfileCommand>>();

            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var sourceRepo = new LocalSourceRepo(this.SourceDir, loggerFactory);
            var ctx = new DockerfileContext
            {
                SourceRepo = sourceRepo,
            };

            var dockerfile = serviceProvider.GetRequiredService<IDockerfileGenerator>().GenerateDockerfile(ctx);
            if (string.IsNullOrEmpty(dockerfile))
            {
                exitCode = ProcessConstants.ExitFailure;
                console.WriteErrorLine("Couldn't generate dockerfile.");
            }
            else
            {
                exitCode = ProcessConstants.ExitSuccess;
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
            }

            return exitCode;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            this.SourceDir = string.IsNullOrEmpty(this.SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(this.SourceDir);
            if (!Directory.Exists(this.SourceDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{this.SourceDir}'.");
                return false;
            }

            // Must provide build image in the format '<image>:<tag>'
            if (!string.IsNullOrEmpty(this.BuildImage))
            {
                var buildImageSplit = this.BuildImage.Split(':');
                if (buildImageSplit.Length != 2)
                {
                    console.WriteErrorLine("Provided build image must be in the format '<image>:<tag>'.");
                    return false;
                }
            }

            // Invalid to specify platform version without platform name
            if (string.IsNullOrEmpty(this.PlatformName) && !string.IsNullOrEmpty(this.PlatformVersion))
            {
                console.WriteErrorLine("Cannot use platform version without specifying platform name also.");
                return false;
            }

            // Invalid to specify runtime platform version without platform name
            if (string.IsNullOrEmpty(this.RuntimePlatformName) && !string.IsNullOrEmpty(this.RuntimePlatformVersion))
            {
                console.WriteErrorLine("Cannot use runtime platform version without specifying runtime platform name also.");
                return false;
            }

            // Check for invalid runtime platform name
            if (!string.IsNullOrEmpty(this.RuntimePlatformVersion))
            {
                if (!this.supportedRuntimePlatforms.Contains(this.RuntimePlatformName))
                {
                    console.WriteLine($"WARNING: Unable to find provided runtime platform name '{this.RuntimePlatformName}' in " +
                                      $"supported list of runtime platform names: {string.Join(", ", this.supportedRuntimePlatforms)}. " +
                                      $"The provided runtime platform name will be used in case this Dockerfile command or image is outdated.");
                }
            }

            // Validate provided port is an integer and is in the range [0, 65535]
            if (!string.IsNullOrEmpty(this.BindPort))
            {
                if (!int.TryParse(this.BindPort, out int port) || port < 0 || port > 65535)
                {
                    console.WriteErrorLine($"Provided bind port '{this.BindPort}' is not valid.");
                    return false;
                }
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
                            // These values are not retrieved through the 'config' api since we do not expect
                            // them to be provided by an end user.
                            options.SourceDir = this.SourceDir;
                            options.ScriptOnly = false;
                            options.DebianFlavor = this.ResolveOsType(options, console);
                            options.ImageType = this.ResolveImageType(options, console);
                        });
                });

            return serviceProviderBuilder.Build();
        }

        private CustomConfigurationSource GetCommandLineConfigSource()
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            commandLineConfigSource.Set(SettingsKeys.BindPort, this.BindPort);
            commandLineConfigSource.Set(SettingsKeys.BuildImage, this.BuildImage);
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