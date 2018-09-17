// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Oryx.BuildScriptGenerator;
    using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
    using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

    [Command(Description = "Generates build scripts for multiple languages.")]
    [Subcommand("languages", typeof(LanguagesCommand))]
    internal class Program
    {
        [Option(
            CommandOptionType.SingleValue,
            Description = "The programming language being used in the provided source code directory.",
            ShortName = "l",
            LongName = "language")]
        public string Language { get; private set; }

        [Argument(0, Description = "The path to the source code directory.")]
        public string SourceCodeFolder { get; private set; }

        [Argument(1, Description = "The path to the build script to be generated.")]
        public string TargetScriptPath { get; private set; }

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static void Exec(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            if (string.IsNullOrEmpty(SourceCodeFolder) || string.IsNullOrEmpty(TargetScriptPath))
            {
                app.ShowHelp();
                return 1;
            }

            IServiceProvider serviceProvider = null;
            ILogger logger = null;
            try
            {
                serviceProvider = new ServiceProviderBuilder()
                    .WithScriptGenerationOptions(this)
                    .Build();

                var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                if (!Directory.Exists(options.SourcePath))
                {
                    console.WriteLine($"Couldn't find directory '{options.SourcePath}'.");
                    return 1;
                }

                var scriptGeneratorProvider = serviceProvider.GetRequiredService<IScriptGeneratorProvider>();
                var sourceRepo = serviceProvider.GetRequiredService<ISourceRepo>();
                var scriptGenerator = scriptGeneratorProvider.GetScriptGenerator(sourceRepo, Language);
                if (scriptGenerator == null)
                {
                    console.WriteLine(
                        "Could not find a script generator which can generate a script for " +
                        $"the code in '{options.SourcePath}'.");
                    return 1;
                }

                var scriptContent = scriptGenerator.GenerateBashScript(sourceRepo);

                File.WriteAllText(options.TargetScriptPath, scriptContent);

                Exec(cmd: "chmod +x " + options.TargetScriptPath);

                console.WriteLine($"Script was generated successfully at '{options.TargetScriptPath}'.");
            }
            catch (InvalidUsageException ex)
            {
                console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred while running this tool:" + Environment.NewLine + ex.ToString());
                console.WriteLine("Oops... An unexpected error has occurred.");
                return 1;
            }
            finally
            {
                // In general it is a good practice to dispose services before this program is
                // exiting, but there's one more reason we would need to do this i.e that the Console
                // logger doesn't write to the console immediately. This is because it runs on a separate
                // thread where it queues up messages and writes the console when the queue reaches a certain
                // threshold.
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            return 0;
        }

        [Command("languages", Description = "Show the list of supported languages.")]
        private class LanguagesCommand
        {
            private int OnExecute(CommandLineApplication app, IConsole console)
            {
                var serviceProvider = new ServiceProviderBuilder()
                    .Build();
                var scriptGeneratorProvider = serviceProvider.GetRequiredService<IScriptGeneratorProvider>();
                foreach (var language in scriptGeneratorProvider.GetScriptGenerators())
                {
                    if (!string.IsNullOrWhiteSpace(language.LanguageName))
                    {
                        if (language.LanguageVersions != null)
                        {
                            var versions = string.Join(", ", language.LanguageVersions);
                            console.WriteLine($"{language.LanguageName}: {versions}");
                        }
                        else
                        {
                            console.WriteLine($"{language.LanguageName}");
                        }
                    }
                }

                return 0;
            }
        }
    }
}