// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Description = "Generates build scripts for multiple languages.")]
    [Subcommand("script", typeof(ScriptCommand))]
    [Subcommand("build", typeof(BuildCommand))]
    [Subcommand("languages", typeof(LanguagesCommand))]
    internal class Program
    {
        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        internal int OnExecute(CommandLineApplication app, IConsole console)
        {
            app.ShowHelp();
            return 0;
        }
    }
}