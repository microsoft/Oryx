// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("script", Description = "Generate script to standard output.")]
    internal class ScriptCommand : BaseCommand
    {
        [Argument(0, Description = "The path to the source code directory.")]
        public string SourceCodeFolder { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The programming language being used in the provided source code directory.",
            ShortName = "l")]
        public string LanguageName { get; set; }

        [Option(
            "--language-version <LANGUAGE_VERSION>",
            CommandOptionType.SingleValue,
            Description = "The version of programming language being used in the provided source code directory.")]
        public string LanguageVersion { get; set; }

        [Option(
            "--log-file <LOG_FILE>",
            CommandOptionType.SingleValue,
            Description = "The file to which logs have to be written to.")]
        public string LogFile { get; set; }

        [Option(
            "--log-level <LOG_LEVEL>",
            CommandOptionType.SingleValue,
            Description = "The minimum log level at which logs should be written. " +
            "Allowed levels: Trace, Debug, Information, Warning, Error, Critical. " +
            "Default level is Warning.")]
        public string LogLevel { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new ScriptGenerator(console, serviceProvider);
            if (!scriptGenerator.TryGenerateScript(out var generatedScript))
            {
                return 1;
            }

            console.WriteLine(generatedScript);

            return 0;
        }

        internal override bool ShowHelp()
        {
            if (string.IsNullOrEmpty(SourceCodeFolder))
            {
                return true;
            }
            return false;
        }

        internal override bool IsValidInput(BuildScriptGeneratorOptions options, IConsole console)
        {
            if (!Directory.Exists(options.SourceCodeFolder))
            {
                console.Error.WriteLine($"Error: Could not find the source code folder '{options.SourceCodeFolder}'.");
                return false;
            }
            
            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptoins(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                SourceCodeFolder,
                LanguageName,
                LanguageVersion,
                LogFile,
                LogLevel);
        }
    }
}
