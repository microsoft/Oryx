// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class PlatformsCommand : CommandBase
    {
        public const string Name = "platforms";
        public const string Description = "Show a list of supported platforms along with their versions and build properties.";

        public PlatformsCommand()
        {
        }

        public PlatformsCommand(PlatformsCommandProperty input)
        {
            this.OutputJson = input.OutputJson;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public bool OutputJson { get; set; }

        public static Command Export(IConsole console)
        {
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);
            var jsonOption = new Option<bool>(OptionArgumentTemplates.PlatformsJsonOutput, OptionArgumentTemplates.PlatformsJsonOutputDescription);

            var command = new Command(Name, Description);
            command.AddOption(jsonOption);
            command.AddOption(logOption);
            command.AddOption(debugOption);

            command.SetHandler(
                (prop) =>
                {
                    var platformsCommand = new PlatformsCommand(prop);
                    return Task.FromResult(platformsCommand.OnExecute(console));
                },
                new PlatformsCommandBinder(
                    outputJson: jsonOption,
                    logPath: logOption,
                    debugMode: debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PlatformsCommand>>();
            var platformInfo = new List<PlatformResult>();
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            using (telemetryClient.LogTimedEvent("ListPlatforms"))
            {
                var availableIPlatforms = serviceProvider.GetRequiredService<IEnumerable<IProgrammingPlatform>>()
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var iPlatform in availableIPlatforms)
                {
                    var platform = new PlatformResult { Name = iPlatform.Name };

                    if (iPlatform.SupportedVersions != null && iPlatform.SupportedVersions.Any())
                    {
                        platform.Versions = SortVersions(iPlatform.SupportedVersions);
                    }

                    var props = iPlatform.GetType().GetCustomAttributes(
                        typeof(BuildPropertyAttribute),
                        inherit: true).OfType<BuildPropertyAttribute>();
                    if (props.Any())
                    {
                        platform.Properties = new Dictionary<string, string>();
                        foreach (var prop in props)
                        {
                            platform.Properties[prop.Name] = prop.Description;
                        }
                    }

                    platformInfo.Add(platform);
                }
            }

            console.WriteLine(this.OutputJson ? JsonConvert.SerializeObject(platformInfo) : FormatResult(platformInfo));
            return ProcessConstants.ExitSuccess;
        }

        internal override IServiceProvider TryGetServiceProvider(IConsole console)
        {
            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            // Override the GetServiceProvider() call in CommandBase to pass the IConsole instance to
            // ServiceProviderBuilder and allow for writing to the console if needed during this command.
            var serviceProviderBuilder = new ServiceProviderBuilder(this.LogFilePath, console)
                .ConfigureServices(services =>
                {
                    // Configure Options related services
                    // We first add IConfiguration to DI so that option services like
                    // `DotNetCoreScriptGeneratorOptionsSetup` services can get it through DI and read from the config
                    // and set the options.
                    services
                        .AddSingleton<IConfiguration>(config)
                        .AddOptionsServices();
                });

            return serviceProviderBuilder.Build();
        }

        private static string FormatResult(IList<PlatformResult> platforms)
        {
            var result = new StringBuilder();

            foreach (PlatformResult platform in platforms)
            {
                var defs = new DefinitionListFormatter();
                defs.AddDefinition("Platform", platform.Name);

                defs.AddDefinition(
                    "Versions",
                    (platform.Versions != null && platform.Versions.Any()) ?
                        string.Join(Environment.NewLine, platform.Versions) : "N/A");

                if (platform.Properties != null && platform.Properties.Any())
                {
                    defs.AddDefinition(
                        "Properties",
                        string.Join(
                            Environment.NewLine,
                            platform.Properties.Select(prop => $"{prop.Key} - {prop.Value}")));
                }

                result.AppendLine(defs.ToString());
            }

            return result.ToString();
        }

        private static IList<string> SortVersions(IEnumerable<string> versions)
        {
            var result = new List<VersionInfo>();
            foreach (var version in versions)
            {
                try
                {
                    result.Add(new VersionInfo(version));
                }
                catch (ArgumentException)
                {
                    // Ignore non-SemVer strings (e.g. 'latest', 'lts')
                }
            }

            result.Sort();
            return result.Select(v => v.DisplayVersion).ToList();
        }

        private class PlatformResult
        {
            public string Name { get; set; }

            public IList<string> Versions { get; set; }

            public IDictionary<string, string> Properties { get; set; }
        }
    }
}
