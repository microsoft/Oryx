// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("oryx", Description = "Generates and runs build scripts for multiple languages.")]
    [Subcommand("build", typeof(BuildCommand))]
    [Subcommand("languages", typeof(LanguagesCommand))]
    internal class Program
    {
        [Option(
            CommandOptionType.NoValue,
            Description = "Print version information.")]
        public bool Version { get; set; }

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        internal int OnExecute(CommandLineApplication app, IConsole console)
        {
            if (Version)
            {
                var version = GetVersion();
                var commit = GetCommit();
                console.WriteLine($"Version: {version}, Commit: {commit}");

                return 0;
            }

            app.ShowHelp();

            return 0;
        }

        private string GetVersion()
        {
            var informationalVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersion != null)
            {
                return informationalVersion.InformationalVersion;
            }
            return null;
        }

        private string GetCommit()
        {
            var commitMetadata = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Where(attr => attr.Key.Equals("GitCommit"))
                    .FirstOrDefault();
            if (commitMetadata != null)
            {
                return commitMetadata.Value;
            }
            return null;
        }
    }
}