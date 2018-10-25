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
    [Command("languages", Description = "Show the list of supported languages and their versions.")]
    internal class LanguagesCommand : BaseCommand
    {
        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerators = serviceProvider.GetRequiredService<IEnumerable<ILanguageScriptGenerator>>();
            scriptGenerators = scriptGenerators
                .OrderBy(sg => sg.SupportedLanguageName, StringComparer.OrdinalIgnoreCase);

            foreach (var scriptGenerator in scriptGenerators)
            {
                if (!string.IsNullOrWhiteSpace(scriptGenerator.SupportedLanguageName))
                {
                    if (scriptGenerator.SupportedLanguageVersions != null)
                    {
                        var sortedVersions = SortVersions(scriptGenerator.SupportedLanguageVersions);
                        console.WriteLine(
                            $"{scriptGenerator.SupportedLanguageName}: {string.Join(", ", sortedVersions)}");
                    }
                    else
                    {
                        console.WriteLine($"{scriptGenerator.SupportedLanguageName}");
                    }
                }
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
