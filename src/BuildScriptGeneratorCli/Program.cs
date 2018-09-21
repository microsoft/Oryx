// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Description = "Generates build scripts for multiple languages.")]
    [Subcommand("languages", typeof(LanguagesCommand))]
    internal partial class Program
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
            CommandOptionType.SingleValue,
            Description = "The programming language being used in the provided source code directory.",
            ShortName = "l")]
        public string LanguageName { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The version of programming language being used in the provided source code directory.",
            ShortName = "lv")]
        public string LanguageVersion { get; set; }

        [Option(
            CommandOptionType.NoValue,
            Description = "Generates only a script and does not do a build.",
            ShortName = "so")]
        public bool ScriptOnly { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The path to the script that needs to be generated.",
            ShortName = "sp")]
        public string ScriptPath { get; set; }

        internal static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        //TODO; write unit tests for this
        [Command("languages", Description = "Show the list of supported languages.")]
        private class LanguagesCommand
        {
            private int OnExecute(CommandLineApplication app, IConsole console)
            {
                var serviceProvider = new ServiceProviderBuilder()
                    .Build();

                var scriptGenerators = serviceProvider.GetRequiredService<IEnumerable<IScriptGenerator>>();
                scriptGenerators = scriptGenerators
                    .OrderBy(sg => sg.SupportedLanguageName, StringComparer.OrdinalIgnoreCase);

                foreach (var scriptGenerator in scriptGenerators)
                {
                    if (!string.IsNullOrWhiteSpace(scriptGenerator.SupportedLanguageName))
                    {
                        if (scriptGenerator.SupportedLanguageVersions != null)
                        {
                            var versions = string.Join(", ", scriptGenerator.SupportedLanguageVersions);
                            console.WriteLine($"{scriptGenerator.SupportedLanguageName}: {versions}");
                        }
                        else
                        {
                            console.WriteLine($"{scriptGenerator.SupportedLanguageName}");
                        }
                    }
                }

                return 0;
            }
        }
    }
}