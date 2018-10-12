// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
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

        [Argument(1, Description = "The destination directory.")]
        public string DestinationDir { get; set; }

        [Option(
            "-i|--intermediate-dir <dir>",
            CommandOptionType.SingleValue,
            Description = "The path to a temporary directory to be used by this tool.")]
        public string IntermediateDir { get; set; }

        [Option(
            "--inline",
            CommandOptionType.NoValue,
            Description = "Perform builds directly in the source directory.")]
        public bool Inline { get; set; }

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
            "--log-file <file>",
            CommandOptionType.SingleValue,
            Description = "The file to which logs have to be written to.")]
        public string LogFile { get; set; }

        [Option(
            "--script-only",
            CommandOptionType.NoValue,
            Description = "Generate script to standard output.")]
        public bool ScriptOnly { get; set; }

        [Option(
            CommandOptionType.NoValue,
            Description = "Replace any existing content in the destination directory.")]
        public bool Force { get; set; }

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
            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var sourceRepo = sourceRepoProvider.GetSourceRepo();
            var scriptGenerator = new ScriptGenerator(console, serviceProvider);
            if (!scriptGenerator.TryGenerateScript(out var scriptContent))
            {
                return 1;
            }

            // Replace any CRLF with LF
            scriptContent = scriptContent.Replace("\r\n", "\n");

            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            if (options.ScriptOnly)
            {
                // Write script content to standard output stream
                console.WriteLine(scriptContent);
                return 0;
            }

            var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();

            // Get the path where the generated script should be written into.
            var scriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "build.sh");

            // Write the content to the script.
            File.WriteAllText(scriptPath, scriptContent);
            logger.LogDebug($"Script was generated successfully at '{scriptPath}'.");

            // Set execute permission on the generated script.
            (var exitCode, var output, var error) = ProcessHelper.RunProcessAndCaptureOutput(
                "chmod",
                arguments: new[] { "+x", scriptPath },
                // Do not provide wait time as the caller can do this themselves.
                waitForExitInSeconds: null);
            if (exitCode != 0)
            {
                console.Error.WriteLine(
                    $"Error: Could not set execute permission on the generated script '{scriptPath}'." +
                    Environment.NewLine +
                    $"Output: {output}" +
                    Environment.NewLine +
                    $"Error: {error}");
                return 1;
            }

            // Run the generated script
            logger.LogDebug($"Running the script '{scriptPath}' ...");
            exitCode = ProcessHelper.RunProcess(
                scriptPath,
                arguments: new[]
                {
                    sourceRepo.RootPath,
                    options.DestinationDir,
                    tempDirectoryProvider.GetTempDirectory(),
                    options.Force.ToString()
                },
                standardOutputHandler: stdOutHandler,
                standardErrorHandler: stdErrHandler,
                // Do not provide wait time as the caller can do this themselves.
                waitForExitInSeconds: null);
            return exitCode;
        }

        internal override bool ShowHelp()
        {
            if (string.IsNullOrEmpty(SourceDir) || string.IsNullOrEmpty(DestinationDir))
            {
                return true;
            }
            return false;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            if (!Directory.Exists(options.SourceDir))
            {
                console.Error.WriteLine($"Error: Could not find the source directory '{options.SourceDir}'.");
                return false;
            }

            if (options.Inline && !string.IsNullOrEmpty(options.IntermediateDir))
            {
                console.Error.WriteLine(
                    "Cannot use 'inline' option when intermediate directory is specified.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(options.Language) && !string.IsNullOrEmpty(options.LanguageVersion))
            {
                console.Error.WriteLine("Cannot use language version without specifying language name also.");
                return false;
            }

            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptoins(BuildScriptGeneratorOptions options)
        {
            options.SourceDir = Path.GetFullPath(SourceDir);
            options.Language = Language;
            options.LanguageVersion = LanguageVersion;

            if (!string.IsNullOrEmpty(DestinationDir))
            {
                options.DestinationDir = Path.GetFullPath(DestinationDir);
            }

            if (!string.IsNullOrEmpty(IntermediateDir))
            {
                options.IntermediateDir = Path.GetFullPath(IntermediateDir);
            }

            options.Inline = Inline;

            // We want to enable logging always, so provide a default log file
            // if not explicitly supplied.
            if (!string.IsNullOrEmpty(LogFile))
            {
                options.LogFile = Path.GetFullPath(LogFile);
            }

            options.MinimumLogLevel = LogLevel.Trace;
            options.ScriptOnly = ScriptOnly;
            options.Force = Force;
        }
    }
}
