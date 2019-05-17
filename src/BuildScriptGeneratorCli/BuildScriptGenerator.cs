// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildScriptGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsole _console;
        private readonly List<ICheckerMessage> _checkerMessageSink;
        private readonly ILogger<BuildScriptGenerator> _logger;

        public BuildScriptGenerator(
            IServiceProvider serviceProvider,
            IConsole console,
            List<ICheckerMessage> checkerMessageSink)
        {
            _console = console;
            _serviceProvider = serviceProvider;
            _checkerMessageSink = checkerMessageSink;
            _logger = _serviceProvider.GetRequiredService<ILogger<BuildScriptGenerator>>();
        }

        public static BuildScriptGeneratorContext CreateContext(
            BuildScriptGeneratorOptions options,
            CliEnvironmentSettings envSettings,
            ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
                Language = options.Language,
                LanguageVersion = options.LanguageVersion,
                Properties = options.Properties,
                EnableCheckers = !envSettings.DisableCheckers,
                EnableDotNetCore = !envSettings.DisableDotNetCore,
                EnableNodeJs = !envSettings.DisableNodeJs,
                EnablePython = !envSettings.DisablePython,
                EnablePhp = !envSettings.DisablePhp,
                DisableMultiPlatformBuild = envSettings.DisableMultiPlatformBuild
            };
        }

        public bool TryGenerateScript(out string generatedScript)
        {
            generatedScript = null;

            try
            {
                var options = _serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                var scriptGen = _serviceProvider.GetRequiredService<IBuildScriptGenerator>();
                var sourceRepoProvider = _serviceProvider.GetRequiredService<ISourceRepoProvider>();
                var environment = _serviceProvider.GetRequiredService<CliEnvironmentSettings>();
                var sourceRepo = sourceRepoProvider.GetSourceRepo();
                var scriptGenCtx = CreateContext(options, environment, sourceRepo);

                scriptGen.GenerateBashScript(scriptGenCtx, out generatedScript, _checkerMessageSink);
                return true;
            }
            catch (InvalidUsageException ex)
            {
                _logger.LogError(ex, "Invalid usage");
                _console.Error.WriteLine(ex.Message);
                return false;
            }
        }
    }
}