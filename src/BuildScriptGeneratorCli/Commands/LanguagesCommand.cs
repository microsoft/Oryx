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
    //TODO; write unit tests for this
    [Command("languages", Description = "Show the list of supported languages.")]
    internal class LanguagesCommand : BaseCommand
    {
        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerators = serviceProvider.GetRequiredService<IEnumerable<IScriptGenerator>>();
            scriptGenerators = scriptGenerators
                .OrderBy(sg => sg.SupportedLanguageName, StringComparer.OrdinalIgnoreCase);

            foreach (var scriptGenerator in scriptGenerators)
            {
                if (!string.IsNullOrWhiteSpace(scriptGenerator.SupportedLanguageName))
                {
                    if (scriptGenerator.SupportedLanguageVersions != null)
                    {
                        var versions = string.Join(", ", scriptGenerator.SupportedLanguageVersions);
                        console.WriteLine($"{scriptGenerator.SupportedLanguageName}: {versions}");
                    }
                    else
                    {
                        console.WriteLine($"{scriptGenerator.SupportedLanguageName}");
                    }
                }
            }

            return 0;
        }
    }
}
