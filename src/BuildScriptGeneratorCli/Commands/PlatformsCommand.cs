// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Show a list of supported platforms along with their versions and build properties.")]
    internal class PlatformsCommand : CommandBase
    {
        public const string Name = "platforms";

        [Option("--json", Description = "Output the supported platform data in JSON format.")]
        public bool OutputJson { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PlatformsCommand>>();
            var platformInfo = new List<PlatformResult>();

            using (logger.LogTimedEvent("ListPlatforms"))
            {
                var availableIPlatforms = serviceProvider.GetRequiredService<IEnumerable<IProgrammingPlatform>>()
                    .Where(programmingPlatform => !string.IsNullOrWhiteSpace(programmingPlatform.Name))
                    .OrderBy(programmingPlatform => programmingPlatform.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var programmingPlatform in availableIPlatforms)
                {
                    var platform = new PlatformResult { Name = programmingPlatform.Name };

                    if (programmingPlatform.SupportedVersions != null && programmingPlatform.SupportedVersions.Any())
                    {
                        platform.Versions = SortVersions(programmingPlatform.SupportedVersions);
                    }

                    var props = programmingPlatform.GetType().GetCustomAttributes(
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

            console.WriteLine(OutputJson ? JsonConvert.SerializeObject(platformInfo) : FormatResult(platformInfo));
            return ProcessConstants.ExitSuccess;
        }

        private string FormatResult(IList<PlatformResult> platforms)
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

        private IList<string> SortVersions(IEnumerable<string> versions)
        {
            var result = new List<SemVer.Version>();
            foreach (var version in versions)
            {
                try
                {
                    result.Add(new SemVer.Version(version));
                }
                catch (ArgumentException)
                {
                    // Ignore non-SemVer strings (e.g. 'latest', 'lts')
                }
            }

            result.Sort();
            return result.Select(v => v.ToString()).ToList();
        }

        private class PlatformResult
        {
            public string Name { get; set; }

            public IList<string> Versions { get; set; }

            public IDictionary<string, string> Properties { get; set; }
        }
    }
}
