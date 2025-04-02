// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
    internal class BuildCommand : BuildCommandBase
    {
        public const string Name = "build";
        public const string Description = "Build an app.";

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
            this.IntermediateDir = input.IntermediateDir;
            this.DestinationDir = input.DestinationDir;
            this.ManifestDir = input.ManifestDir;
            this.SourceDir = input.SourceDir;
            this.PlatformName = input.Platform;
            this.PlatformVersion = input.PlatformVersion;
            this.ShouldPackage = input.ShouldPackage;
            this.OsRequirements = input.OsRequirements;
            this.AppType = input.AppType;
            this.BuildCommandsFileName = input.BuildCommandFile;
            this.CompressDestinationDir = input.CompressDestinationDir;
            this.Properties = input.Property;
            this.DynamicInstallRootDir = input.DynamicInstallRootDir;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;

            // Handling obselete options
            if (input.LanguageVersionWasSet)
            {
                this.LanguageVersion = input.LanguageVersion;
            }

            if (input.LanguageWasSet)
            {
                this.LanguageName = input.LanguageName;
            }
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

        public static Command Export(IConsole console)
        {
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);
            var sourceDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.SourceDir,
                description: OptionArgumentTemplates.SourceDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var platformOption = new Option<string>(OptionArgumentTemplates.Platform, OptionArgumentTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionArgumentTemplates.PlatformVersion, OptionArgumentTemplates.PlatformVersionDescription);
            var packageOption = new Option<bool>(OptionArgumentTemplates.Package, OptionArgumentTemplates.PackageDescription);
            var osReqOption = new Option<string>(OptionArgumentTemplates.OsRequirements, OptionArgumentTemplates.OsRequirementsDescription);
            var appTypeOption = new Option<string>(OptionArgumentTemplates.AppType, OptionArgumentTemplates.AppTypeDescription);
            var buildCommandFileNameOption = new Option<string>(OptionArgumentTemplates.BuildCommandsFileName, OptionArgumentTemplates.BuildCommandsFileNameDescription);
            var compressDestDirOption = new Option<bool>(OptionArgumentTemplates.CompressDestinationDir, OptionArgumentTemplates.CompressDestinationDirDescription);
            var propertyOption = new Option<string[]>(OptionArgumentTemplates.Property, OptionArgumentTemplates.PropertyDescription);
            var dynamicInstallRootDirOption = new Option<string>(OptionArgumentTemplates.DynamicInstallRootDir, OptionArgumentTemplates.DynamicInstallRootDirDescription);

            // Hiding Language Option because it is obselete
            var languageOption = new Option<string>(OptionArgumentTemplates.Language, OptionArgumentTemplates.LanguageDescription);
            languageOption.IsHidden = true;

            // LanguageVer Option is obselete
            var languageVerOption = new Option<string>(OptionArgumentTemplates.LanguageVersion, OptionArgumentTemplates.LanguageVersionDescription);
            languageVerOption.IsHidden = true;

            var intermediateDirOption = new Option<string>(OptionArgumentTemplates.IntermediateDir, OptionArgumentTemplates.IntermediateDirDescription);

            // destinationDirOption is "--output" in CLI
            // destinationDir matches with its property name
            var destinationDirOption = new Option<string>(aliases: OptionArgumentTemplates.Output, OptionArgumentTemplates.OutputDescription);

            var manifestDirOption = new Option<string>(OptionArgumentTemplates.ManifestDir, OptionArgumentTemplates.ManifestDirDescription);

            var command = new Command(Name, Description)
            {
                sourceDirArgument,
                intermediateDirOption,
                destinationDirOption,
                manifestDirOption,
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
                logOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    // InvocationContext provided in SetHandler
                    var buildCommand = new BuildCommand(prop);
                    var returnCode = buildCommand.OnExecute(console);
                    return Task.FromResult(returnCode);
                },
                new BuildCommandBinder(
                    languageName: languageOption,
                    languageVersion: languageVerOption,
                    intermediateDir: intermediateDirOption,
                    destinationDir: destinationDirOption,
                    manifestDir: manifestDirOption,
                    sourceDir: sourceDirArgument,
                    platform: platformOption,
                    platformVersion: platformVersionOption,
                    package: packageOption,
                    osRequirements: osReqOption,
                    appType: appTypeOption,
                    buildCommandFile: buildCommandFileNameOption,
                    compressDestinationDir: compressDestDirOption,
                    property: propertyOption,
                    dynamicInstallRootDir: dynamicInstallRootDirOption,
                    logPath: logOption,
                    debugMode: debugOption));
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
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
            var buildOperationId = telemetryClient.StartOperation(BuildOperationName(environment));

            var sourceRepo = serviceProvider.GetRequiredService<ISourceRepoProvider>().GetSourceRepo();
            var sourceRepoCommitId = GetSourceRepoCommitId(environment, sourceRepo, logger, telemetryClient);

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
            telemetryClient.LogEvent("BuildRequested", buildEventProps);

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
            using (var stopwatch = telemetryClient.LogTimedEvent("GenerateBuildScript"))
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
            telemetryClient.LogTrace("Build script written to file");
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
                new TextSpanEventLogger(logger, this.measurableStdOutSpans, telemetryClient),
                new PipDownloadEventLogger(logger, telemetryClient),
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
                console.Error.WriteLine(line);
                buildScriptOutput.AppendLine(line);
            };

            // Run the generated script
            int exitCode;
            using (var timedEvent = telemetryClient.LogTimedEvent("RunBuildScript", buildEventProps))
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
                console.WriteErrorLine("Cannot use platform version without specifying platform name also.");
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
                    console.WriteErrorLine(
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
                    console.WriteErrorLine(
                        $"Intermediate directory '{options.IntermediateDir}' cannot be " +
                        $"same as the source directory '{options.SourceDir}'.");
                    return false;
                }

                // Intermediate directory cannot be a sub-directory of the source directory
                if (DirectoryHelper.IsSubDirectory(options.IntermediateDir, options.SourceDir))
                {
                    logger.LogError(
                        "Intermediate directory cannot be a child of the source directory.");
                    console.WriteErrorLine(
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
                            // These values are not retrieved through the 'config' api since we do not expect
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

        private static string GetSourceRepoCommitId(IEnvironment env, ISourceRepo repo, ILogger<BuildCommand> logger, TelemetryClient telemetryClient)
        {
            string commitId = env.GetEnvironmentVariable(ExtVarNames.ScmCommitIdEnvVarName);

            if (string.IsNullOrEmpty(commitId))
            {
                using (var timedEvent = telemetryClient.LogTimedEvent("GetGitCommitId"))
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