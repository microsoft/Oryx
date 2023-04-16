// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildScriptGenerator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConsole console;
        private readonly List<ICheckerMessage> checkerMessageSink;
        private readonly ILogger<BuildScriptGenerator> logger;
        private readonly string operationId;

        public BuildScriptGenerator(
            IServiceProvider serviceProvider,
            IConsole console,
            List<ICheckerMessage> checkerMessageSink,
            string operationId)
        {
            this.console = console;
            this.serviceProvider = serviceProvider;
            this.checkerMessageSink = checkerMessageSink;
            this.logger = this.serviceProvider.GetRequiredService<ILogger<BuildScriptGenerator>>();
            this.operationId = operationId;
        }

        public static BuildScriptGeneratorContext CreateContext(IServiceProvider serviceProvider, string operationId)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var sourceRepoProvider = serviceProvider.GetRequiredService<ISourceRepoProvider>();
            var envSettings = serviceProvider.GetRequiredService<CliEnvironmentSettings>();

            return new BuildScriptGeneratorContext
            {
                OperationId = operationId,
                SourceRepo = sourceRepoProvider.GetSourceRepo(),
                Properties = options.Properties,
                ManifestDir = options.ManifestDir,
                BuildCommandsFileName = options.BuildCommandsFileName,
            };
        }

        public bool TryGenerateScript(out string generatedScript, out Exception exception)
        {
            generatedScript = null;
            exception = null;

            try
            {
                var scriptGenCtx = CreateContext(this.serviceProvider, this.operationId);
                var scriptGen = this.serviceProvider.GetRequiredService<IBuildScriptGenerator>();

                scriptGen.GenerateBashScript(scriptGenCtx, out generatedScript, this.checkerMessageSink);
                return true;
            }
            catch (InvalidUsageException ex)
            {
                exception = ex;
                this.logger.LogError(ex, "Invalid usage");
                this.console.WriteErrorLine(ex.Message);
                return false;
            }
        }
    }
}