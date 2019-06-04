// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultRunScriptGenerator : IRunScriptGenerator
    {
        private const string TempScriptPath = "/tmp/run.sh";

        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultRunScriptGenerator> _logger;

        public RunScriptGenerator(IEnumerable<IProgrammingPlatform> platforms, ILogger<DefaultRunScriptGenerator> logger)
        {
            _programmingPlatforms = platforms;
            _logger = logger;
        }

        public string GenerateBashScript(string targetPlatformName, RunScriptGeneratorOptions opts)
        {
            var targetPlatform = _programmingPlatforms.Where(
                p => string.Equals(
                    p.Name,
                    targetPlatformName,
                    StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (targetPlatform == null)
            {
                throw new UnsupportedLanguageException($"Platform '{targetPlatformName}' is not supported.");
            }

            return RunStartupScriptGeneratorForPlatform(targetPlatform, opts);
        }

        private string RunStartupScriptGeneratorForPlatform(IProgrammingPlatform platform, RunScriptGeneratorOptions opts)
        {
            var scriptGenPath = FilePaths.RunScriptGeneratorDir + "/" + platform.Name;
            
            (int exitCode, string stdout, string stderr) = ProcessHelper.RunProcess(
                scriptGenPath,
                new[] { "-appPath", opts.SourceRepo.RootPath, "-output", TempScriptPath },
                Environment.CurrentDirectory,
                TimeSpan.FromSeconds(10));

            if (exitCode != ProcessConstants.ExitSuccess)
            {
                _logger.LogError("{scriptGenPath} returned {exitCode}", scriptGenPath, exitCode);
                throw new Exception("{scriptGenPath} failed");
            }

            return File.ReadAllText(TempScriptPath);
        }
    }
}