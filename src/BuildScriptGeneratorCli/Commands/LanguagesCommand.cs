// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(
        "languages",
        Description = "Show the list of supported languages and other information like versions, properties etc.")]
    internal class LanguagesCommand : BaseCommand
    {
        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            // Note: Ensure all these labels have equal length
            var languageLabel =
                "Language    : ";
            var versionLabel =
                "Versions    : ";
            var propertiesLabel =
                "Properties  : ";
            var padding = new string(' ', languageLabel.Length);

            var scriptGenerators = serviceProvider.GetRequiredService<IEnumerable<ILanguageScriptGenerator>>();
            scriptGenerators = scriptGenerators
                .OrderBy(sg => sg.SupportedLanguageName, StringComparer.OrdinalIgnoreCase);

            foreach (var scriptGenerator in scriptGenerators)
            {
                if (string.IsNullOrWhiteSpace(scriptGenerator.SupportedLanguageName))
                {
                    continue;
                }

                if (scriptGenerator.SupportedLanguageVersions != null)
                {
                    var sortedVersions = SortVersions(scriptGenerator.SupportedLanguageVersions);
                    console.WriteLine($"{languageLabel}{scriptGenerator.SupportedLanguageName}");
                    console.WriteLine($"{versionLabel}{sortedVersions.First()}");
                    console.Write(string.Join(Environment.NewLine, sortedVersions.Skip(1).Select(
                        v => $"{padding}{v}")));
                    console.WriteLine();
                }
                else
                {
                    console.WriteLine($"{scriptGenerator.SupportedLanguageName}");
                }

                // get properties
                var properties = scriptGenerator.GetType()
                    .GetCustomAttributes(typeof(BuildPropertyAttribute), inherit: true)
                    .OfType<BuildPropertyAttribute>();
                if (!properties.Any())
                {
                    console.WriteLine();
                    continue;
                }

                console.WriteLine($"{propertiesLabel}Name, Description");
                console.Write(string.Join(Environment.NewLine, properties.Select(
                    p => $"{padding}{p.Name}, {p.Description}")));
                console.WriteLine();
                console.WriteLine();
            }

            return 0;
        }

        private IEnumerable<string> SortVersions(IEnumerable<string> versions)
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
                    // ignore non semantic version based versions like 'latest' or 'lts'
                }
            }
            result.Sort();

            return result.Select(v => v.ToString());
        }
    }
}
