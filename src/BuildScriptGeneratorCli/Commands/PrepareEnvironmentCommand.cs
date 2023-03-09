﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Java;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Detector.Python;
using Microsoft.Oryx.Detector.Ruby;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Sets up environment by detecting and installing platforms.")]
    internal class PrepareEnvironmentCommand : CommandBase
    {
        public const string Name = "prep";
        private const string SourceDirectoryTemplate = "-s|--src";
        private const string SkipDetectionTemplate = "--skip-detection";
        private const string PlatformsAndVersionsTemplate = "--platforms-and-versions";
        private const string PlatformsAndVersionsFileTemplate = "--platforms-and-versions-file";

        [Option(
            SourceDirectoryTemplate,
            CommandOptionType.SingleValue,
            Description = "The source directory.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option(
            SkipDetectionTemplate,
            CommandOptionType.NoValue,
            Description = "Skip detection of platforms and install the requested platforms.")]
        public bool SkipDetection { get; set; }

        [Option(
            PlatformsAndVersionsTemplate,
            CommandOptionType.SingleValue,
            Description =
            "Comma separated values of platforms and versions to be installed. " +
            "Example: dotnet=3.1.200,php=7.4.5,node=2.3")]
        public string PlatformsAndVersions { get; set; }

        [Option(
            PlatformsAndVersionsFileTemplate,
            CommandOptionType.SingleValue,
            Description =
            "A .env file which contains list of platforms and the versions that need to be installed. " +
            "Example: \ndotnet=3.1.200\nphp=7.4.5\nnode=2.3")]
        public string PlatformsAndVersionsFile { get; set; }

        // To enable unit testing
        internal static bool TryValidateSuppliedPlatformsAndVersions(
            IEnumerable<IProgrammingPlatform> availablePlatforms,
            string suppliedPlatformsAndVersions,
            string suppliedPlatformsAndVersionsFile,
            IConsole console,
            BuildScriptGeneratorContext context,
            out List<PlatformDetectorResult> results)
        {
            results = new List<PlatformDetectorResult>();

            if (string.IsNullOrEmpty(suppliedPlatformsAndVersions)
                && string.IsNullOrEmpty(suppliedPlatformsAndVersionsFile))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(suppliedPlatformsAndVersionsFile)
                && !File.Exists(suppliedPlatformsAndVersionsFile))
            {
                console.WriteErrorLine($"Supplied file '{suppliedPlatformsAndVersionsFile}' does not exist.");
                return false;
            }

            IEnumerable<string> platformsAndVersions;
            if (string.IsNullOrEmpty(suppliedPlatformsAndVersions))
            {
                var lines = File.ReadAllLines(suppliedPlatformsAndVersionsFile);
                platformsAndVersions = lines
                    .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'));
            }
            else
            {
                // Example: python,dotnet=3.1.300, node=12.3, Python=3.7.3
                platformsAndVersions = suppliedPlatformsAndVersions
                    .Trim()
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(nv => nv.Trim());
            }

            var platformNames = availablePlatforms.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var platformNameAndVersion in platformsAndVersions)
            {
                var parts = platformNameAndVersion.Split("=", StringSplitOptions.RemoveEmptyEntries);

                // It is OK to have a platform name without version in which case a default version of the platform
                // is installed.
                string platformName = null;
                string version = null;
                platformName = parts[0].Trim();
                if (parts.Length == 2)
                {
                    version = parts[1].Trim();
                }

                if (!platformNames.ContainsKey(platformName))
                {
                    console.WriteErrorLine(
                        $"Platform name '{platformName}' is not valid. Make sure platform name matches one of the " +
                        $"following names: {string.Join(", ", platformNames.Keys)}");
                    return false;
                }

                var platformDetectorResult = GetPlatformDetectorResult(platformName, version);

                var platform = platformNames[platformName];
                platform.ResolveVersions(context, platformDetectorResult);

                results.Add(platformDetectorResult);
            }

            return true;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PrepareEnvironmentCommand>>();
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            var beginningOutputLog = GetBeginningCommandOutputLog();
            console.WriteLine(beginningOutputLog);

            int exitCode;
            using (var timedEvent = telemetryClient.LogTimedEvent("EnvSetupCommand"))
            {
                var context = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);

                IEnumerable<PlatformDetectorResult> detectedPlatforms = null;
                if (this.SkipDetection)
                {
                    console.WriteLine(
                        $"Skipping platform detection since '{SkipDetectionTemplate}' switch was used...");

                    var platforms = serviceProvider.GetRequiredService<IEnumerable<IProgrammingPlatform>>();
                    if (TryValidateSuppliedPlatformsAndVersions(
                        platforms,
                        this.PlatformsAndVersions,
                        this.PlatformsAndVersionsFile,
                        console,
                        context,
                        out var results))
                    {
                        detectedPlatforms = results;
                    }
                    else
                    {
                        console.WriteErrorLine(
                            $"Invalid value for switch '{PlatformsAndVersionsTemplate}'.");
                        return ProcessConstants.ExitFailure;
                    }
                }
                else
                {
                    var detector = serviceProvider.GetRequiredService<DefaultPlatformsInformationProvider>();
                    var platformInfos = detector.GetPlatformsInfo(context);
                    if (!platformInfos.Any())
                    {
                        return ProcessConstants.ExitFailure;
                    }

                    detectedPlatforms = platformInfos.Select(pi => pi.DetectorResult);
                }

                var environmentScriptProvider = serviceProvider.GetRequiredService<PlatformsInstallationScriptProvider>();
                var snippet = environmentScriptProvider.GetBashScriptSnippet(context, detectedPlatforms);

                var scriptBuilder = new StringBuilder()
                    .AppendLine($"#!{FilePaths.Bash}")
                    .AppendLine("set -e")
                    .AppendLine();

                if (!string.IsNullOrEmpty(snippet))
                {
                    scriptBuilder
                        .AppendLine("echo")
                        .AppendLine("echo Setting up environment...")
                        .AppendLine("echo")
                        .AppendLine(snippet)
                        .AppendLine("echo")
                        .AppendLine("echo Done setting up environment.")
                        .AppendLine("echo");
                }

                // Create temporary file to store script
                // Get the path where the generated script should be written into.
                var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
                var tempScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "setupEnvironment.sh");
                var script = scriptBuilder.ToString();
                File.WriteAllText(tempScriptPath, script);
                timedEvent.AddProperty(nameof(tempScriptPath), tempScriptPath);

                if (this.DebugMode)
                {
                    console.WriteLine($"Temporary script @ {tempScriptPath}:");
                    console.WriteLine("---");
                    console.WriteLine(scriptBuilder);
                    console.WriteLine("---");
                }

                var environment = serviceProvider.GetRequiredService<IEnvironment>();
                var shellPath = environment.GetEnvironmentVariable("BASH") ?? FilePaths.Bash;

                exitCode = ProcessHelper.RunProcess(
                    shellPath,
                    new[] { tempScriptPath },
                    options.SourceDir,
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
                            console.Error.WriteLine(args.Data);
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
            if (!this.IsValidInput(console))
            {
                return null;
            }

            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var configBuilder = new ConfigurationBuilder();

            if (string.IsNullOrEmpty(this.PlatformsAndVersionsFile))
            {
                // Gather all the values supplied by the user in command line
                this.SourceDir = string.IsNullOrEmpty(this.SourceDir) ?
                    Directory.GetCurrentDirectory() : Path.GetFullPath(this.SourceDir);
                configBuilder.AddIniFile(Path.Combine(this.SourceDir, Constants.BuildEnvironmentFileName), optional: true);
            }
            else
            {
                string versionsFilePath;
                if (this.PlatformsAndVersionsFile.StartsWith("/"))
                {
                    versionsFilePath = Path.GetFullPath(this.PlatformsAndVersionsFile);
                }
                else
                {
                    versionsFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        Path.GetFullPath(this.PlatformsAndVersionsFile));
                }

                if (!File.Exists(versionsFilePath))
                {
                    throw new FileNotFoundException(
                        $"Could not find the file provided to the '{PlatformsAndVersionsFileTemplate}' switch.",
                        versionsFilePath);
                }

                configBuilder.AddIniFile(versionsFilePath, optional: false);
            }

            configBuilder
                .AddEnvironmentVariables()
                .Add(GetCommandLineConfigSource());

            var config = configBuilder.Build();

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

        private static PlatformDetectorResult GetPlatformDetectorResult(string name, string version)
        {
            var result = new PlatformDetectorResult();
            switch (name)
            {
                case DotNetCoreConstants.PlatformName:
                    result = new DotNetCorePlatformDetectorResult();
                    break;
                case NodeConstants.PlatformName:
                    result = new NodePlatformDetectorResult();
                    break;
                case PythonConstants.PlatformName:
                    result = new PythonPlatformDetectorResult();
                    break;
                case HugoConstants.PlatformName:
                    result = new PlatformDetectorResult();
                    break;
                case PhpConstants.PlatformName:
                    result = new PhpPlatformDetectorResult();
                    break;
                case JavaConstants.PlatformName:
                    result = new JavaPlatformDetectorResult();
                    break;
                case RubyConstants.PlatformName:
                    result = new RubyPlatformDetectorResult();
                    break;
            }

            result.Platform = name;
            result.PlatformVersion = version;

            return result;
        }

        private static CustomConfigurationSource GetCommandLineConfigSource()
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            return commandLineConfigSource;
        }

        private bool IsValidInput(IConsole console)
        {
            if (!this.SkipDetection && string.IsNullOrEmpty(this.SourceDir))
            {
                console.WriteErrorLine("Source directory is required.");
                return false;
            }

            if (this.SkipDetection
                && string.IsNullOrEmpty(this.PlatformsAndVersions)
                && string.IsNullOrEmpty(this.PlatformsAndVersionsFile))
            {
                console.WriteErrorLine("Platform names and versions are required.");
                return false;
            }

            return true;
        }
    }
}
