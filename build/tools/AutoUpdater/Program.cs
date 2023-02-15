// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;

namespace AutoUpdater
{
    [Command("autoupdater", Description = "Tool to run checks and send out PRs if necessary.")]
    [Subcommand(typeof(GitHubRunnersCachedImagesCheckCommand))]
    class Program
    {
        internal int OnExecute(CommandLineApplication app, IConsole console)
        {
            app.ShowHelp();

            return 0;
        }

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
    }
}
