// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("build", Description = "Generates script and builds the code present in the source code directory.")]
    internal class BuildCommand : BaseCommand
    {
        [Argument(0, Description = "The path to the source code directory.")]
        public string SourceCodeFolder { get; set; }

        [Argument(1, Description = "The path to output folder.")]
        public string OutputFolder { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The path to a temporary folder used by this tool.",
            ShortName = "i")]
        public string IntermediateFolder { get; set; }

        [Option(
            "--no-intermediate-folder",
            CommandOptionType.NoValue,
            Description = "Do not use intermediate folder to copy source folder's content." +
            " By default an intermediate folder is used.")]
        public bool DoNotUseIntermediateFolder { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The programming language being used in the provided source code directory.",
            ShortName = "l")]
        public string LanguageName { get; set; }

        [Option(
            "--language-version",
            CommandOptionType.SingleValue,
            Description = "The version of programming language being used in the provided source code directory.")]
        public string LanguageVersion { get; set; }


        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var sourceRepo = sourceRepoProvider.GetSourceRepo();
            var scriptGenerator = new ScriptGenerator(console, serviceProvider);
            if (!scriptGenerator.TryGenerateScript(out var scriptContent))
            {
                return 1;
            }

            // Get the path where the generated script should be written into.
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var scriptPath = Path.Combine(options.TempDirectory, "build.sh");

            // Write the content to the script.
            File.WriteAllText(scriptPath, scriptContent);
            console.WriteLine($"Script was generated successfully at '{scriptPath}'.");

            // Set execute permission on the generated script.
            (var exitCode, var output) = ProcessHelper.RunProcessAndCaptureOutput(
                "chmod",
                arguments: new[] { "+x", scriptPath });
            if (exitCode != 0)
            {
                console.WriteLine(
                    $"Error: Could not set execute permission on the generated script '{scriptPath}'." +
                    Environment.NewLine +
                    $"Output: {output}");
                return 1;
            }

            // Run the generated script
            console.WriteLine();
            console.WriteLine($"Running the script '{scriptPath}' ...");
            (exitCode, output) = ProcessHelper.RunProcessAndCaptureOutput(
                scriptPath,
                arguments: new[]
                {
                    sourceRepo.RootPath,
                    options.OutputFolder
                },
                waitForExitInSeconds: (int)TimeSpan.FromMinutes(10).TotalSeconds);
            console.WriteLine();
            console.WriteLine(output);

            return exitCode;
        }

        internal override bool ShowHelp()
        {
            if (string.IsNullOrEmpty(SourceCodeFolder) || string.IsNullOrEmpty(OutputFolder))
            {
                return true;
            }
            return false;
        }

        internal override bool IsValidInput(BuildScriptGeneratorOptions options, IConsole console)
        {
            if (!Directory.Exists(options.SourceCodeFolder))
            {
                console.WriteLine($"Error: Could not find the source code folder '{options.SourceCodeFolder}'.");
                return false;
            }

            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptoins(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder: SourceCodeFolder,
                outputFolder: OutputFolder,
                intermediateFolder: IntermediateFolder,
                DoNotUseIntermediateFolder,
                LanguageName,
                LanguageVersion);
        }
    }
}
