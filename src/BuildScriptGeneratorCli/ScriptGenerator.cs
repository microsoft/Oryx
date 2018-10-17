// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class ScriptGenerator
    {
        private readonly IConsole _console;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScriptGenerator> _logger;

        public ScriptGenerator(
            IConsole console,
            IServiceProvider serviceProvider)
        {
            _console = console;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<ScriptGenerator>>();
        }

        public bool TryGenerateScript(out string generatedScript)
        {
            generatedScript = null;

            try
            {
                var options = _serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                var scriptGeneratorProvider = _serviceProvider.GetRequiredService<IScriptGeneratorProvider>();
                var sourceRepoProvider = _serviceProvider.GetRequiredService<ISourceRepoProvider>();

                var sourceRepo = sourceRepoProvider.GetSourceRepo();
                var scriptGeneratorContext = new ScriptGeneratorContext
                {
                    SourceRepo = sourceRepo,
                    Language = options.Language,
                    LanguageVersion = options.LanguageVersion,
                    DestinationDir = options.DestinationDir,
                };

                // Get script generator
                var scriptGenerator = scriptGeneratorProvider.GetScriptGenerator(scriptGeneratorContext);
                if (scriptGenerator == null)
                {
                    _console.Error.WriteLine(
                        "Error: Could not find a script generator which can generate a script for " +
                        $"the code in '{options.SourceDir}'.");
                    return false;
                }

                generatedScript = scriptGenerator.GenerateBashScript(scriptGeneratorContext);

                // Replace any CRLF with LF
                generatedScript = generatedScript.Replace("\r\n", "\n");

                return true;
            }
            catch (InvalidUsageException ex)
            {
                _console.Error.WriteLine(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while running this tool:" + Environment.NewLine + ex.ToString());
                _console.Error.WriteLine("Oops... An unexpected error has occurred.");
                return false;
            }
        }
    }
}
