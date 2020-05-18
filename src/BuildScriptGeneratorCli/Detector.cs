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
    internal class Detector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsole _console;
        private readonly List<ICheckerMessage> _checkerMessageSink;
        private readonly ILogger<Detector> _logger;

        public Detector(
            IServiceProvider serviceProvider,
            IConsole console,
            List<ICheckerMessage> checkerMessageSink)
        {
            _console = console;
            _serviceProvider = serviceProvider;
            _checkerMessageSink = checkerMessageSink;
            _logger = _serviceProvider.GetRequiredService<ILogger<Detector>>();
        }

        public static DetectorContext CreateContext(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<DetectorOptions>>().Value;
            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var envSettings = serviceProvider.GetRequiredService<CliEnvironmentSettings>();

            return new DetectorContext
            {
                SourceRepo = sourceRepoProvider.GetSourceRepo(),
            };
        }

        //Add TryDetect() which throws exception if not able to get detection results.
    }
}