// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.Threading.Tasks;

namespace AutoUpdater
{
    internal class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.Name = "autoupdater";
            rootCommand.Description = "Tool to run checks and send out PRs if necessary.";
            rootCommand.AddCommand(GitHubRunnersCachedImagesCheckCommand.Export());

            return await rootCommand.InvokeAsync(args);
        }
    }
}
