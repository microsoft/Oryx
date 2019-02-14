// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
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

            // This will be an App Service app name if Oryx was invoked by Kudu
            var appName = Environment.GetEnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName) ?? ".oryx";
            var buildOpId = logger.StartOperation(appName);
            console.WriteLine("Build Operation ID: {0}", buildOpId);

            console.WriteLine("Oryx Version      : {0}, Commit: {1}", Program.GetVersion(), Program.GetCommit());

            var scriptExecutor = serviceProvider.GetRequiredService<IScriptExecutor>();
            var sourceRepo = serviceProvider.GetRequiredService<ISourceRepoProvider>().GetSourceRepo();
            var commitId = GetSourceRepoCommitId(console, sourceRepo, logger);

            // Try writing the ID to a file in the source directory
            try
            {
                using (logger.LogTimedEvent("WriteBuildIdFile"))
                using (var idFileWriter = new StreamWriter(Path.Combine(sourceRepo.RootPath, Common.FilePaths.BuildIdFileName)))
                {
                    idFileWriter.Write(buildOpId);
                }
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Exception caught while trying to write build ID file");
            }

            var environmentSettingsProvider = serviceProvider.GetRequiredService<IEnvironmentSettingsProvider>();
            if (!environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings))
            {
                return Constants.ExitFailure;
            }

            // Generate build script
            string scriptContent;
            using (var stopwatch = logger.LogTimedEvent("GenerateBuildScript"))
            {
                var scriptGenerator = new ScriptGenerator(console, serviceProvider);
                if (!scriptGenerator.TryGenerateScript(out scriptContent))
                {
                    stopwatch.AddProperty("failed", "true");
                    return Constants.ExitFailure;
                }
            }

            // Get the path where the generated script should be written into.
            var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
            var buildScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "build.sh");

            // Write build script to selected path
            File.WriteAllText(buildScriptPath, scriptContent);
            logger.LogTrace("Build script written to file");

            var buildEventProps = new Dictionary<string, string>()
            {
                { "oryxVersion", Program.GetVersion() },
                { "oryxCommitId", Program.GetCommit() },
                { "oryxCommandLine", string.Join(' ', serviceProvider.GetRequiredService<IEnvironment>().GetCommandLineArgs()) },
                { nameof(commitId), commitId },
                { "scriptPath", buildScriptPath },
                { "envVars", string.Join(",", GetEnvVarNames(serviceProvider.GetRequiredService<IEnvironment>())) },
            };

            // Run the generated script
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            int exitCode;
            using (var timedEvent = logger.LogTimedEvent("RunBuildScript", buildEventProps))
            {
                exitCode = scriptExecutor.ExecuteScript(
                    buildScriptPath,
                    new[] { sourceRepo.RootPath, options.DestinationDir ?? string.Empty },
                    workingDirectory: sourceRepo.RootPath,
                    stdOutHandler,
                    stdErrHandler);
            }

            if (exitCode != Constants.ExitSuccess)
            {
                logger.LogError("Build script exited with {exitCode}", exitCode);
                return exitCode;
            }

            return Constants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<BuildCommand>>();

            if (!Directory.Exists(options.SourceDir))
            {
                logger.LogError("Could not find the source directory {srcDir}", options.SourceDir);
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
                    logger.LogError("Intermediate directory {intermediateDir} cannot be a child of {srcDir}", options.IntermediateDir, options.SourceDir);
                    console.Error.WriteLine(
                        $"Intermediate directory '{options.IntermediateDir}' cannot be a " +
                        $"sub-directory of source directory '{options.SourceDir}'.");
                    return false;
                }

                // If intermediate folder is provided, we assume user doesn't want to modify it. In this case,
                // we do not want the output folder to be part of source directory.
                if (!string.IsNullOrEmpty(options.DestinationDir) && IsSubDirectory(options.DestinationDir, options.SourceDir))
                {
                    logger.LogError("Destination directory {dstDir} cannot be a child of {srcDir}", options.DestinationDir, options.SourceDir);
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
        /// <param name="dir1">The directory to be checked as subdirectory.</param>
        /// <param name="dir2">The directory to be tested as the parent.</param>
        /// <returns>true if <c>dir1</c> is a sub-directory of <c>dir2</c>, false otherwise.</returns>
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

        private string GetSourceRepoCommitId(IConsole console, ISourceRepo repo, ILogger<BuildCommand> logger)
        {
            string commitId;
            using (var timedEvent = logger.LogTimedEvent("GetGitCommitId"))
            {
                commitId = repo.GetGitCommitId();
                timedEvent.AddProperty(nameof(commitId), commitId);
                if (!string.IsNullOrWhiteSpace(commitId))
                {
                    // Spacing is meant to equalize the length to "Build Operation ID"
                    console.WriteLine("Repository Commit : {0}", commitId);
                }
            }

            return commitId;
        }

        private string[] GetEnvVarNames([CanBeNull] IEnvironment env)
        {
            var envVarKeyCollection = env?.GetEnvironmentVariables()?.Keys;
            if (envVarKeyCollection == null)
            {
                return new string[] { };
            }

            string[] envVarNames = new string[envVarKeyCollection.Count];
            envVarKeyCollection.CopyTo(envVarNames, 0);
            return envVarNames;
        }
    }
}
