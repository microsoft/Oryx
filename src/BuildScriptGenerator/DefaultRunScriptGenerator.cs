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
using NLog.Targets;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Runs the run script generator for the target platform.
    /// Assumes these external binaries are named exactly as their corresponding platforms.
    /// </summary>
    internal class DefaultRunScriptGenerator : IRunScriptGenerator
    {
        private static readonly TimeSpan RunScriptGeneratorTimeout = TimeSpan.FromSeconds(5);

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

        public string GenerateBashScript(RunScriptGeneratorContext ctx)
        {
            if (ctx.SourceRepo == null)
            {
                throw new ArgumentNullException(nameof(ctx.SourceRepo), "Source repository must be supplied.");
            }

            IProgrammingPlatform targetPlatform = null;
            if (!string.IsNullOrEmpty(ctx.Platform))
            {
                targetPlatform = _programmingPlatforms
                    .Where(p => p.Name.EqualsIgnoreCase(ctx.Platform))
                    .FirstOrDefault();

                if (targetPlatform == null)
                {
                    throw new UnsupportedPlatformException($"Platform '{ctx.Platform}' is not supported.");
                }
            }
            else
            {
                _logger.LogDebug("No platform provided for run-script command; attempting to determine platform...");
                foreach (var platform in _programmingPlatforms)
                {
                    _logger.LogDebug($"Checking if platform '{platform.Name}' is compatible...");
                    var detectionResult = platform.Detect(ctx);
                    if (detectionResult != null)
                    {
                        _logger.LogDebug($"Detected platform '{detectionResult.Language}' with version '{detectionResult.LanguageVersion}'.");
                        if (string.IsNullOrEmpty(detectionResult.LanguageVersion))
                        {
                            throw new UnsupportedVersionException($"Couldn't detect a version for platform '{detectionResult.Language}' in the repo.");
                        }

                        targetPlatform = platform;
                        break;
                    }
                }

                if (targetPlatform == null)
                {
                    throw new UnsupportedPlatformException("Unable to determine the platform for the given repo.");
                }
            }

            return RunStartupScriptGeneratorForPlatform(targetPlatform, ctx);
        }

        private string RunStartupScriptGeneratorForPlatform(IProgrammingPlatform plat, RunScriptGeneratorContext ctx)
        {
            var scriptGenPath = FilePaths.RunScriptGeneratorDir + "/" + plat.Name;

            var scriptGenArgs = new List<string> { "-appPath", ctx.SourceRepo.RootPath, "-output", _tempScriptPath };
            if (ctx.PassThruArguments != null)
            {
                scriptGenArgs.AddRange(ctx.PassThruArguments);
            }

            (int exitCode, string stdout, string stderr) = ProcessHelper.RunProcess(
                scriptGenPath, scriptGenArgs, Environment.CurrentDirectory, RunScriptGeneratorTimeout);

            if (exitCode != ProcessConstants.ExitSuccess)
            {
                _logger.LogError("Generated run script returned exit code '{exitCode}'", exitCode);
                throw new Exception($"{scriptGenPath} failed");
            }

            return File.ReadAllText(_tempScriptPath);
        }
    }
}