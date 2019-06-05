// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Runs the run script generator for the target platform.
    /// Assumes these external binaries are named exactly as their corresponding platforms.
    /// </summary>
    internal class DefaultRunScriptGenerator : IRunScriptGenerator
    {
        private readonly string _tempScriptPath = Path.Combine(Path.GetTempPath(), "run.sh");

        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly ILogger<DefaultRunScriptGenerator> _logger;

        public DefaultRunScriptGenerator(
            IEnumerable<IProgrammingPlatform> platforms,
            ILogger<DefaultRunScriptGenerator> logger)
        {
            _programmingPlatforms = platforms;
            _logger = logger;
        }

        public string GenerateBashScript(string targetPlatformName, RunScriptGeneratorOptions opts)
        {
            var targetPlatform = _programmingPlatforms
                .Where(p => p.Name.EqualsIgnoreCase(targetPlatformName))
                .FirstOrDefault();

            if (targetPlatform == null)
            {
                throw new UnsupportedLanguageException($"Platform '{targetPlatformName}' is not supported.");
            }

            return RunStartupScriptGeneratorForPlatform(targetPlatform, opts);
        }

        private string RunStartupScriptGeneratorForPlatform(IProgrammingPlatform plat, RunScriptGeneratorOptions opts)
        {
            var scriptGenPath = FilePaths.RunScriptGeneratorDir + "/" + plat.Name;

            (int exitCode, string stdout, string stderr) = ProcessHelper.RunProcess(
                scriptGenPath,
                new[] { "-appPath", opts.SourceRepo.RootPath, "-output", _tempScriptPath },
                Environment.CurrentDirectory,
                TimeSpan.FromSeconds(10));

            if (exitCode != ProcessConstants.ExitSuccess)
            {
                _logger.LogError("{scriptGenPath} returned {exitCode}", scriptGenPath, exitCode);
                throw new Exception("{scriptGenPath} failed");
            }

            return File.ReadAllText(_tempScriptPath);
        }
    }
}