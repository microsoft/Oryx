// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Oryx.BuildScriptGenerator;
    using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

    internal class Program
    {
        [Argument(0, Description = "The path to the source code directory.")]
        [Required]
        public string SourceCodeFolder { get; private set; }

        [Argument(1, Description = "The path to the build script to be generated.")]
        [Required]
        public string TargetScriptPath { get; private set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "The programming language being used in the provided source code directory.",
            ShortName = "l",
            LongName = "language")]
        public string Language { get; private set; }

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private int OnExecute()
        {
            IServiceProvider serviceProvider = null;
            ILogger logger = null;
            try
            {
                serviceProvider = GetServiceProvider();
                var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                if (!Directory.Exists(options.SourcePath))
                {
                    Console.WriteLine($"Couldn't find directory '{options.SourcePath}'.");
                    return 1;
                }

                var scriptGeneratorProvider = serviceProvider.GetRequiredService<IScriptGeneratorProvider>();

                var scriptGenerator = scriptGeneratorProvider.GetScriptGenerator();
                if (scriptGenerator == null)
                {
                    Console.WriteLine(
                        "Could not find a script generator which can generate a script for " +
                        $"the code in '{options.SourcePath}'.");
                    return 1;
                }

                var scriptContent = scriptGenerator.GenerateShScript();
                var targetScriptPath = Path.GetFullPath(options.TargetScriptPath);
                File.WriteAllText(targetScriptPath, scriptContent);

                Console.WriteLine($"Script was generated successfully at '{this.TargetScriptPath}'.");
            }
            catch (InvalidUsageException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred while running this tool:" + Environment.NewLine + ex.ToString());
                Console.WriteLine("Oops... An unexpected error has occurred.");
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

        private IServiceProvider GetServiceProvider()
        {
            var configuration = GetConfiguration();

            IServiceCollection services = new ServiceCollection();
            services
                .AddBuildScriptGeneratorServices()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
                });

            services.Configure<BuildScriptGeneratorOptions>(options =>
            {
                options.SourcePath = Path.GetFullPath(this.SourceCodeFolder);
                options.Language = this.Language;
                options.TargetScriptPath = this.TargetScriptPath;
            });

            return services.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration()
        {
            // The order of 'Add' is important here.
            // The values provided at commandline override any values provided in environment variables
            // and values provided in environment variables override any values provided in appsettings.json.
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables();

            return configurationBuilder.Build();
        }
    }
}