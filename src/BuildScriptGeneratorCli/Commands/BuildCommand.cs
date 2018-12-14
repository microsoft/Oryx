// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("build", Description = "Generate and run build scripts.")]
    internal class BuildCommand : BaseCommand
    {
        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        [Option(
            "-i|--intermediate-dir <dir>",
            CommandOptionType.SingleValue,
            Description = "The path to a temporary directory to be used by this tool.")]
        public string IntermediateDir { get; set; }

        [Option(
            "-l|--language <name>",
            CommandOptionType.SingleValue,
            Description = "The name of the programming language being used in the provided source directory.")]
        public string Language { get; set; }

        [Option(
            "--language-version <version>",
            CommandOptionType.SingleValue,
            Description = "The version of programming language being used in the provided source directory.")]
        public string LanguageVersion { get; set; }

        [Option(
            "-o|--output <dir>",
            CommandOptionType.SingleValue,
            Description = "The destination directory.")]
        public string DestinationDir { get; set; }

        [Option(
            "-p|--property <key-value>",
            CommandOptionType.MultipleValue,
            Description = "Additional information used by this tool to generate and run build scripts.")]
        public string[] Properties { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            // By default we do not want to direct the standard output and error and let users of this tool to do it
            // themselves.
            return Execute(serviceProvider, console, stdOutHandler: null, stdErrHandler: null);
        }

        // To enable unit testing
        internal int Execute(
            IServiceProvider serviceProvider,
            IConsole console,
            DataReceivedEventHandler stdOutHandler,
            DataReceivedEventHandler stdErrHandler)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();

            var opName = Environment.GetEnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName) ?? "oryx"; // This will be an App Service app name if Oryx was invoked by Kudu
            console.WriteLine("Build Operation ID: {0}", logger.StartOperation(opName));

            var scriptExecutor = serviceProvider.GetRequiredService<IScriptExecutor>();
            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var sourceRepo = sourceRepoProvider.GetSourceRepo();

            using (var stopwatch = logger.LogTimedEvent("GetGitCommitId"))
            {
                string commitId = sourceRepo.GetGitCommitId();
                stopwatch.AddProperty(nameof(commitId), commitId);
                if (!string.IsNullOrWhiteSpace(commitId))
                {
                    console.WriteLine("Git Commit ID:      {0}", commitId); // Spacing is meant to equalize the length to "Build Operation ID"
                }
            }

            var environmentSettingsProvider = serviceProvider.GetRequiredService<IEnvironmentSettingsProvider>();
            if (!environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings))
            {
                return Constants.ExitFailure;
            }

            // Run pre-build script
            var exitCode = Constants.ExitFailure;
            if (!string.IsNullOrEmpty(environmentSettings.PreBuildScriptPath))
            {
                logger.LogInformation("Executing pre-build script {PreBuildScript}...", environmentSettings.PreBuildScriptPath);

                var scriptDirectory = new FileInfo(environmentSettings.PreBuildScriptPath).Directory.FullName;
                exitCode = scriptExecutor.ExecuteScript(
                    environmentSettings.PreBuildScriptPath,
                    args: null,
                    workingDirectory: scriptDirectory,
                    stdOutHandler,
                    stdOutHandler);

                if (exitCode != Constants.ExitSuccess)
                {
                    return exitCode;
                }
            }

            // Run actual build
            var scriptGenerator = new ScriptGenerator(console, serviceProvider);
            if (!scriptGenerator.TryGenerateScript(out var scriptContent))
            {
                return Constants.ExitFailure;
            }

            // Get the path where the generated script should be written into.
            var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
            var buildScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "build.sh");

            // Write the content to the script.
            File.WriteAllText(buildScriptPath, scriptContent);
            logger.LogDebug("Script was generated successfully at {BuildScript}; running it...", buildScriptPath);

            // Run the generated script
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            exitCode = scriptExecutor.ExecuteScript(
                buildScriptPath,
                new[] { sourceRepo.RootPath, options.DestinationDir ?? string.Empty },
                workingDirectory: sourceRepo.RootPath,
                stdOutHandler,
                stdErrHandler);

            if (exitCode != Constants.ExitSuccess)
            {
                return exitCode;
            }

            // Run post-build script
            if (!string.IsNullOrEmpty(environmentSettings.PostBuildScriptPath))
            {
                logger.LogInformation("Executing post-build script {PostBuildScript}...", environmentSettings.PostBuildScriptPath);

                var scriptDirectory = new FileInfo(environmentSettings.PostBuildScriptPath).Directory.FullName;
                exitCode = scriptExecutor.ExecuteScript(
                    environmentSettings.PostBuildScriptPath,
                    args: null,
                    workingDirectory: scriptDirectory,
                    stdOutHandler,
                    stdErrHandler);

                if (exitCode != Constants.ExitSuccess)
                {
                    return exitCode;
                }
            }

            return Constants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();

            if (!Directory.Exists(options.SourceDir))
            {
                logger.LogError("Could not find the source directory {SrcDir}", options.SourceDir);
                console.Error.WriteLine($"Error: Could not find the source directory '{options.SourceDir}'.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(options.Language) && !string.IsNullOrEmpty(options.LanguageVersion))
            {
                logger.LogError("Cannot use lang version without lang name");
                console.Error.WriteLine("Cannot use language version without specifying language name also.");
                return false;
            }

            if (!string.IsNullOrEmpty(options.IntermediateDir))
            {
                // Intermediate directory cannot be a sub-directory of the source directory
                if (IsSubDirectory(options.IntermediateDir, options.SourceDir))
                {
                    logger.LogError("Intermediate directory {IntermediateDir} cannot be a child of {SrcDir}", options.IntermediateDir, options.SourceDir);
                    console.Error.WriteLine(
                        $"Intermediate directory '{options.IntermediateDir}' cannot be a " +
                        $"sub-directory of source directory '{options.SourceDir}'.");
                    return false;
                }

                // If intermediate folder is provided, we assume user doesn't want to modify it. In this case,
                // we do not want the output folder to be part of source directory.
                if (!string.IsNullOrEmpty(options.DestinationDir) && IsSubDirectory(options.DestinationDir, options.SourceDir))
                {
                    logger.LogError("Destination directory {DstDir} cannot be a child of {SrcDir}", options.DestinationDir, options.SourceDir);
                    console.Error.WriteLine(
                        $"Destination directory '{options.DestinationDir}' cannot be a " +
                        $"sub-directory of source directory '{options.SourceDir}'.");
                    return false;
                }
            }

            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                SourceDir,
                DestinationDir,
                IntermediateDir,
                Language,
                LanguageVersion,
                scriptOnly: false,
                Properties);
        }

        /// <summary>
        /// Checks if <paramref name="dir1"/> is a sub-directory of <paramref name="dir2"/>.
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        internal bool IsSubDirectory(string dir1, string dir2)
        {
            var dir1Segments = dir1.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var dir2Segments = dir2.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            if (dir1Segments.Length < dir2Segments.Length)
            {
                return false;
            }

            // If dir1 is really a subset of dir2, then we should expect all
            // segments of dir2 appearing in dir1 and in exact order.
            for (var i = 0; i < dir2Segments.Length; i++)
            {
                // we want case-sensitive search
                if (dir1Segments[i] != dir2Segments[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
