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
    [Subcommand("script", typeof(ScriptCommand))]
    internal class Program
    {
        [Option(CommandOptionType.NoValue, Description = "Print version information.")]
        public bool Version { get; set; }

        internal int OnExecute(CommandLineApplication app, IConsole console)
        {
            if (Version)
            {
                var version = GetVersion();
                var commit = GetCommit();
                console.WriteLine($"Version: {version}, Commit: {commit}");

                return Constants.ExitSuccess;
            }

            app.ShowHelp();

            return Constants.ExitSuccess;
        }

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        internal static string GetVersion()
        {
            var informationalVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersion != null)
            {
                return informationalVersion.InformationalVersion;
            }

            return null;
        }

        internal static string GetCommit()
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