// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildCommand : BuildCommandBase
    {
        // Beginning and ending markers for build script output spans that should be time measured
        private readonly TextSpan[] measurableStdOutSpans =
        {
            new TextSpan(
                "RunPreBuildScript",
                Oryx.BuildScriptGenerator.Constants.PreBuildCommandPrologue,
                Oryx.BuildScriptGenerator.Constants.PreBuildCommandEpilogue),
            new TextSpan(
                "RunPostBuildScript",
                Oryx.BuildScriptGenerator.Constants.PostBuildCommandPrologue,
                Oryx.BuildScriptGenerator.Constants.PostBuildCommandEpilogue),
        };

        private bool languageVersionWasSet;
        private bool languageWasSet;

        public BuildCommand()
        {
        }

        public BuildCommand(BuildCommandProperty input)
        {
            this.LanguageName = input.LanguageName;
            this.LanguageVersion = input.LanguageVersion;
            this.IntermediateDir = input.IntermediateDir;
            this.DestinationDir = input.DestinationDir;
            this.ManifestDir = input.ManifestDir;
            this.SourceDir = input.SourceDir;
            this.PlatformName = input.PlatformName;
            this.PlatformVersion = input.PlatformVersion;
            this.ShouldPackage = input.ShouldPackage;
            this.OsRequirements = input.OsRequirements;
            this.AppType = input.AppType;
            this.BuildCommandsFileName = input.BuildCommandsFileName;
            this.CompressDestinationDir = input.CompressDestinationDir;
            this.Properties = input.Properties;
            this.DynamicInstallRootDir = input.DynamicInstallRootDir;
            this.LogFilePath = input.LogFilePath;
            this.DebugMode = input.DebugMode;
        }

        public string IntermediateDir { get; set; }

        public string LanguageName
        {
            get => this.PlatformName;
            set
            {
                this.PlatformName = value;
                this.languageWasSet = true;
            }
        }

        public string LanguageVersion
        {
            get => this.PlatformVersion;
            set
            {
                this.PlatformVersion = value;
                this.languageVersionWasSet = true;
            }
        }

        public string DestinationDir { get; set; }

        public string ManifestDir { get; set; }

        public static Command Export()
        {
            var logOption = new Option<string>(OptionTemplates.Log, OptionTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionTemplates.Debug, OptionTemplates.DebugDescription);
            var sourceDirArgument = new Argument<string>("sourceDir", "The source directory.");
            var platformOption = new Option<string>(OptionTemplates.Platform, OptionTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionTemplates.PlatformVersion, OptionTemplates.PlatformVersionDescription);
            var packageOption = new Option<bool>(OptionTemplates.Package, OptionTemplates.PackageDescription);
            var osReqOption = new Option<string>(OptionTemplates.OsRequirements, OptionTemplates.OsRequirementsDescription);
            var appTypeOption = new Option<string>(OptionTemplates.AppType, OptionTemplates.AppTypeDescription);
            var buildCommandFileNameOption = new Option<string>(OptionTemplates.BuildCommandsFileName, OptionTemplates.BuildCommandsFileNameDescription);
            var compressDestDirOption = new Option<bool>(OptionTemplates.CompressDestinationDir, OptionTemplates.CompressDestinationDirDescription);
            var propertyOption = new Option<string[]>(aliases: new[] { "-p", OptionTemplates.Property }, OptionTemplates.PropertyDescription);
            var dynamicInstallRootDirOption = new Option<string>(OptionTemplates.DynamicInstallRootDir, OptionTemplates.DynamicInstallRootDirDescription);
            var languageOption = new Option<string>(aliases: new[] { "-l", OptionTemplates.Language }, OptionTemplates.LanguageDescription);
            var languageVerOption = new Option<string>(OptionTemplates.LanguageVersion, OptionTemplates.LanguageVersionDescription);
            var intermediateDirOption = new Option<string>(aliases: new[] { "-i", OptionTemplates.IntermediateDir }, OptionTemplates.IntermediateDirDescription);
            var outputOption = new Option<string>(aliases: new[] { "-o", OptionTemplates.Output }, OptionTemplates.OutputDescription);
            var manifestDirOption = new Option<string>(OptionTemplates.ManifestDir, OptionTemplates.ManifestDirDescription);

            var command = new Command("build", "Build an app.")
            {
                logOption,
                debugOption,
                sourceDirArgument,
                platformOption,
                platformVersionOption,
                packageOption,
                osReqOption,
                appTypeOption,
                buildCommandFileNameOption,
                compressDestDirOption,
                propertyOption,
                dynamicInstallRootDirOption,
                languageOption,
                languageVerOption,
                intermediateDirOption,
                outputOption,
                manifestDirOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    // InvocationContext provided in SetHandler
                    var buildCommand = new BuildCommand(prop);
                    buildCommand.OnExecute();
                },
                new BuildCommandBinder(
                    languageOption,
                    languageVerOption,
                    intermediateDirOption,
                    outputOption,
                    manifestDirOption,
                    sourceDirArgument,
                    platformOption,
                    platformVersionOption,
                    packageOption,
                    osReqOption,
                    appTypeOption,
                    buildCommandFileNameOption,
                    compressDestDirOption,
                    propertyOption,
                    dynamicInstallRootDirOption,
                    logOption,
                    debugOption));
            return command;
        }

        public static string BuildOperationName(IEnvironment env)
        {
            string result = LoggingConstants.DefaultOperationName;

            LoggingConstants.EnvTypeOperationNamePrefix.TryGetValue(env.Type, out string prefix);
            LoggingConstants.OperationNameSourceEnvVars.TryGetValue(env.Type, out string opNameSrcVarName);
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(opNameSrcVarName))
            {
                return result;
            }

            string opName = env.GetEnvironmentVariable(opNameSrcVarName);
            if (!string.IsNullOrWhiteSpace(opName))
            {
                result = $"{prefix}:{opName}";
            }

            return result;
        }

        internal static IDictionary<string, string> ProcessProperties(string[] properties)
        {
            return BuildScriptGeneratorOptionsHelper.ProcessProperties(properties);
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var environment = serviceProvider.GetRequiredService<IEnvironment>();
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();
            var buildOperationId = logger.StartOperation(BuildOperationName(environment));

            var sourceRepo = serviceProvider.GetRequiredService<ISourceRepoProvider>().GetSourceRepo();
            var sourceRepoCommitId = GetSourceRepoCommitId(environment, sourceRepo, logger);

            var oryxVersion = Program.GetVersion();
            var oryxCommitId = Program.GetMetadataValue(Program.GitCommit);
            var oryxReleaseTagName = Program.GetMetadataValue(Program.ReleaseTagName);

            var buildEventProps = new Dictionary<string, string>()
            {
                { "oryxVersion", oryxVersion },
                { "oryxCommitId", oryxCommitId },
                { "oryxReleaseTagName", oryxReleaseTagName },
                {
                    "oryxCommandLine",
                    string.Join(' ', environment.GetCommandLineArgs())
                },
                { "sourceRepoCommitId", sourceRepoCommitId },
                { "platformName", this.PlatformName },
            };

            logger.LogEvent("BuildRequested", buildEventProps);

            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            var beginningOutputLog = GetBeginningCommandOutputLog();
            console.WriteLine(beginningOutputLog);
            var buildInfo = new DefinitionListFormatter();
            buildInfo.AddDefinition("Build Operation ID", buildOperationId);
            if (!string.IsNullOrWhiteSpace(sourceRepoCommitId))
            {
                buildInfo.AddDefinition("Repository Commit", sourceRepoCommitId);
            }

            if (!string.IsNullOrWhiteSpace(options.DebianFlavor))
            {
                buildInfo.AddDefinition("OS Type", options.DebianFlavor);
            }

            if (!string.IsNullOrWhiteSpace(options.ImageType))
            {
                buildInfo.AddDefinition("Image Type", options.ImageType);
            }

            console.WriteLine(buildInfo.ToString());

            // Generate build script
            string scriptContent;
            Exception exception;
            using (var stopwatch = logger.LogTimedEvent("GenerateBuildScript"))
            {
                var checkerMessages = new List<ICheckerMessage>();
                var scriptGenerator = new BuildScriptGenerator(
                    serviceProvider, console, checkerMessages, buildOperationId);

                var generated = scriptGenerator.TryGenerateScript(out scriptContent, out exception);
                stopwatch.AddProperty("generateSucceeded", generated.ToString());

                if (checkerMessages.Count > 0)
                {
                    var messageFormatter = new DefinitionListFormatter();
                    checkerMessages.ForEach(msg => messageFormatter.AddDefinition(msg.Level.ToString(), msg.Content));
                    console.WriteLine(messageFormatter.ToString());
                }
                else
                {
                    logger.LogDebug("No checker messages emitted");
                }

                if (!generated)
                {
                    if (exception != null)
                    {
                        return ProcessExitCodeHelper.GetExitCodeForException(exception);
                    }

                    return ProcessConstants.ExitFailure;
                }
            }

            // Get the path where the generated script should be written into.
            var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
            var buildScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "build.sh");

            // Write build script to selected path
            File.WriteAllText(buildScriptPath, scriptContent);
            logger.LogTrace("Build script written to file");
            if (this.DebugMode)
            {
                console.WriteLine($"Build script content:\n{scriptContent}");
            }

            // Merge the earlier build event properties
            buildEventProps = new Dictionary<string, string>(buildEventProps)
            {
                { "scriptPath", buildScriptPath },
                { "envVars", string.Join(",", GetEnvVarNames(environment)) },
                { "osType", options.DebianFlavor },
                { "imageType", options.ImageType },
            };

            var buildScriptOutput = new StringBuilder();
            var stdOutEventLoggers = new ITextStreamProcessor[]
            {
                new TextSpanEventLogger(logger, this.measurableStdOutSpans),
                new PipDownloadEventLogger(logger),
            };

            DataReceivedEventHandler stdOutBaseHandler = (sender, args) =>
            {
                string line = args.Data;
                if (line == null)
                {
                    return;
                }

                console.WriteLine(line);
                buildScriptOutput.AppendLine(line);

                foreach (var processor in stdOutEventLoggers)
                {
                    // Catch any exception and log them instead of failing this build since whatever these processors
                    // do are not really relevant to the actual build of the app.
                    try
                    {
                        processor.ProcessLine(line);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            $"An error occurred when trying to process the line '{line}' from standard " +
                            $"out using the  '{processor.GetType()}' processor.");
                    }
                }
            };

            DataReceivedEventHandler stdErrBaseHandler = (sender, args) =>
            {
                string line = args.Data;
                if (line == null)
                {
                    return;
                }

                // Not using IConsole.WriteErrorLine intentionally, to keep the child's error stream intact
                console.WriteLine(line);
                buildScriptOutput.AppendLine(line);
            };

            // Run the generated script
            int exitCode;
            using (var timedEvent = logger.LogTimedEvent("RunBuildScript", buildEventProps))
            {
                console.WriteLine(string.Empty);
                exitCode = serviceProvider.GetRequiredService<IScriptExecutor>().ExecuteScript(
                    buildScriptPath,
                    new[]
                    {
                        sourceRepo.RootPath,
                        options.DestinationDir ?? string.Empty,
                        options.IntermediateDir ?? string.Empty,
                    },
                    workingDirectory: sourceRepo.RootPath,
                    stdOutBaseHandler,
                    stdErrBaseHandler);

                timedEvent.AddProperty("exitCode", exitCode.ToString());
            }

            if (exitCode != ProcessConstants.ExitSuccess)
            {
                logger.LogLongMessage(
                    LogLevel.Error,
                    header: "Error running build script",
                    buildScriptOutput.ToString(),
                    new Dictionary<string, object>
                    {
                        ["buildExitCode"] = exitCode,
                        ["oryxVersion"] = oryxVersion,
                        ["oryxReleaseTagName"] = oryxReleaseTagName,
                    });
                return exitCode;
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();

            if (this.languageWasSet)
            {
                logger.LogWarning("Deprecated option '--language' used");
                console.WriteLine("Warning: the deprecated option '--language' was used.");
            }

            if (this.languageVersionWasSet)
            {
                logger.LogWarning("Deprecated option '--language-version' used");
                console.WriteLine("Warning: the deprecated option '--language-version' was used.");
            }

            // Invalid to specify platform version without platform name
            if (string.IsNullOrEmpty(options.PlatformName) && !string.IsNullOrEmpty(options.PlatformVersion))
            {
                logger.LogError("Cannot use lang version without lang name");
                console.WriteLine("Cannot use platform version without specifying platform name also.");
                return false;
            }

            if (!string.IsNullOrEmpty(options.AppType))
            {
                var appType = options.AppType.ToLower();
                if (!string.Equals(appType, Constants.FunctionApplications)
                    && !string.Equals(appType, Constants.StaticSiteApplications)
                    && !string.Equals(appType, Constants.WebApplications))
                {
                    logger.LogError($"Invalid value for AppType: '{options.AppType}'.");
                    console.WriteLine(
                        $"Invalid value '{options.AppType}' for switch '--apptype'. " +
                        $"Valid values are '{Constants.StaticSiteApplications}' or " +
                        $"'{Constants.FunctionApplications}' or '{Constants.WebApplications}'");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(options.IntermediateDir))
            {
                if (DirectoryHelper.AreSameDirectories(options.IntermediateDir, options.SourceDir))
                {
                    logger.LogError(
                        "Intermediate directory cannot be same as the source directory.");
                    console.WriteLine(
                        $"Intermediate directory '{options.IntermediateDir}' cannot be " +
                        $"same as the source directory '{options.SourceDir}'.");
                    return false;
                }

                // Intermediate directory cannot be a sub-directory of the source directory
                if (DirectoryHelper.IsSubDirectory(options.IntermediateDir, options.SourceDir))
                {
                    logger.LogError(
                        "Intermediate directory cannot be a child of the source directory.");
                    console.WriteLine(
                        $"Intermediate directory '{options.IntermediateDir}' cannot be a " +
                        $"sub-directory of source directory '{options.SourceDir}'.");
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
            this.ManifestDir = string.IsNullOrEmpty(this.ManifestDir) ? null : Path.GetFullPath(this.ManifestDir);
            this.IntermediateDir = string.IsNullOrEmpty(this.IntermediateDir) ? null : Path.GetFullPath(this.IntermediateDir);
            this.DestinationDir = string.IsNullOrEmpty(this.DestinationDir) ? null : Path.GetFullPath(this.DestinationDir);
            this.BuildCommandsFileName = string.IsNullOrEmpty(this.BuildCommandsFileName) ?
                FilePaths.BuildCommandsFileName : this.BuildCommandsFileName;
            var buildProperties = ProcessProperties(this.Properties);

            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var config = new ConfigurationBuilder()
                .AddIniFile(Path.Combine(this.SourceDir, Constants.BuildEnvironmentFileName), optional: true)
                .AddEnvironmentVariables()
                .Add(this.GetCommandLineConfigSource(buildProperties))
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
                            options.IntermediateDir = this.IntermediateDir;
                            options.DestinationDir = this.DestinationDir;
                            options.ManifestDir = this.ManifestDir;
                            options.Properties = buildProperties;
                            options.ScriptOnly = false;
                            options.DebianFlavor = this.ResolveOsType(options, console);
                            options.ImageType = this.ResolveImageType(options, console);
                        });
                });

            return serviceProviderBuilder.Build();
        }

        private static string GetSourceRepoCommitId(IEnvironment env, ISourceRepo repo, ILogger<BuildCommand> logger)
        {
            string commitId = env.GetEnvironmentVariable(ExtVarNames.ScmCommitIdEnvVarName);

            if (string.IsNullOrEmpty(commitId))
            {
                using (var timedEvent = logger.LogTimedEvent("GetGitCommitId"))
                {
                    commitId = repo.GetGitCommitId();
                    timedEvent.AddProperty(nameof(commitId), commitId);
                }
            }

            return commitId;
        }

        private static string[] GetEnvVarNames([CanBeNull] IEnvironment env)
        {
            var envVarKeyCollection = env?.GetEnvironmentVariables()?.Keys;
            if (envVarKeyCollection == null)
            {
                return Array.Empty<string>();
            }

            string[] envVarNames = new string[envVarKeyCollection.Count];
            envVarKeyCollection.CopyTo(envVarNames, 0);
            return envVarNames;
        }

        private CustomConfigurationSource GetCommandLineConfigSource(
            IDictionary<string, string> buildProperties)
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            SetValueIfNotNullOrEmpty(SettingsKeys.PlatformName, this.PlatformName);
            SetValueIfNotNullOrEmpty(SettingsKeys.PlatformVersion, this.PlatformVersion);

            // Set the platform key and version in the format that they are represented in other sources
            // (like environment variables and build.env file).git
            // This is so that this enables Configuration api to apply the hierarchical config.
            // Example: "--platform python --platform-version 3.6" will win over "PYTHON_VERSION=3.7"
            // in environment variable
            SetPlatformVersion(this.PlatformName, this.PlatformVersion);

            commandLineConfigSource.Set(SettingsKeys.CreatePackage, this.ShouldPackage.ToString());
            SetValueIfNotNullOrEmpty(SettingsKeys.RequiredOsPackages, this.OsRequirements);
            SetValueIfNotNullOrEmpty(SettingsKeys.AppType, this.AppType);
            commandLineConfigSource.Set(SettingsKeys.CompressDestinationDir, this.CompressDestinationDir.ToString());
            SetValueIfNotNullOrEmpty(SettingsKeys.DynamicInstallRootDir, this.DynamicInstallRootDir);
            SetValueIfNotNullOrEmpty(SettingsKeys.BuildCommandsFileName, this.BuildCommandsFileName);

            if (buildProperties != null)
            {
                foreach (var pair in buildProperties)
                {
                    commandLineConfigSource.Set(pair.Key, pair.Value);
                }
            }

            return commandLineConfigSource;

            void SetPlatformVersion(string platformName, string platformVersion)
            {
                if (string.IsNullOrEmpty(platformName))
                {
                    return;
                }

                commandLineConfigSource.Set(SettingsKeys.PlatformName, platformName);

                // Set the platform version only if it is present otherwise we would be overwriting any
                // value that is set by earlier configuration sources.
                if (!string.IsNullOrEmpty(platformVersion))
                {
                    platformName = platformName == "nodejs" ? "node" : platformName;
                    var platformVersionKey = $"{platformName}_version".ToUpper();
                    commandLineConfigSource.Set(platformVersionKey, platformVersion);
                }
            }

            void SetValueIfNotNullOrEmpty(string key, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    commandLineConfigSource.Set(key, value);
                }
            }
        }
    }
}