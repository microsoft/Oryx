// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpPlatform : IProgrammingPlatform
    {
        private readonly PhpScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPhpVersionProvider _pythonVersionProvider;
        private readonly ILogger<PhpPlatform> _logger;
        private readonly PhpLanguageDetector _detector;

        public PhpPlatform(
            IOptions<PhpScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPhpVersionProvider pythonVersionProvider,
            ILogger<PhpPlatform> logger,
            PhpLanguageDetector detector)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _logger = logger;
            _detector = detector;
        }

        public string Name => PhpConstants.PhpName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPhpVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext ctx)
        {
            _logger.LogDebug("Selected PHP version: {phpVer}", ctx.PhpVersion);
            bool composerFileExists = false;

            if (ctx.SourceRepo.FileExists(PhpConstants.ComposerFileName))
            {
                composerFileExists = true;

                try
                {
                    dynamic composerFile = SourceRepoFileHelpers.ReadJsonObjectFromFile(ctx.SourceRepo, PhpConstants.ComposerFileName);
                    if (composerFile?.require != null)
                    {
                        Newtonsoft.Json.Linq.JObject deps = composerFile?.require;
                        var depSpecs = deps.ToObject<IDictionary<string, string>>();
                        _logger.LogDependencies(this.Name, ctx.PhpVersion, depSpecs.Select(kv => kv.Key + kv.Value));
                    }
                }
                catch (Exception exc)
                {
                    // Leave malformed composer.json files for Composer to handle.
                    // This prevents Oryx from erroring out when Composer itself might be able to tolerate the file.
                    _logger.LogWarning(exc, $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName}");
                }
            }

            var props = new PhpBashBuildSnippetProperties { ComposerFileExists = composerFileExists };
            string snippet = TemplateHelpers.Render(TemplateHelpers.TemplateResource.PhpBuildSnippet, props, _logger);
            return new BuildScriptSnippet { BashBuildScriptSnippet = snippet };
        }

        public bool IsEnabled(BuildScriptGeneratorContext ctx)
        {
            return ctx.EnablePhp;
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public string GenerateBashRunScript(RunScriptGeneratorOptions opts)
        {
            return string.Empty;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PhpConstants.PhpName] = targetPlatformVersion;
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.PhpVersion = version;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir()
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir()
        {
            return Array.Empty<string>();
        }
    }
}