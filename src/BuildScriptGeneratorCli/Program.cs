// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Oryx.BuildScriptGenerator;

    internal class Program
    {
        private static int Main(string[] args)
        {
            IServiceProvider serviceProvider = null;
            ILogger logger = null;
            try
            {
                var commandLineArgs = new CommandLineArgs(args);
                serviceProvider = GetServiceProvider(commandLineArgs);
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

                Console.WriteLine($"Script was generated successfully at '{commandLineArgs.TargetScriptPath}'.");
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

        private static IServiceProvider GetServiceProvider(CommandLineArgs commandLineArgs)
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
                options.SourcePath = Path.GetFullPath(commandLineArgs.SourceCodeFolder);
                options.Language = commandLineArgs.Language;
                options.TargetScriptPath = commandLineArgs.TargetScriptPath;
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
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            return configurationBuilder.Build();
        }
    }
}