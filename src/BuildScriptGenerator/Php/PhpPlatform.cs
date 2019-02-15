// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpPlatform : IProgrammingPlatform
    {
        public const string ComposerInstallLine = "$php $COMPOSER_PHAR install";

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

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext context)
        {
            _logger.LogDebug("Selected PHP version: {phpVer}", context.PhpVersion);

            //_logger.LogDependencies(
            //    "PHP",
            //    phpVersion,
            //    context.SourceRepo.ReadAllLines(PhpConstants.RequirementsFileName)
            //    .Where(line => !line.TrimStart().StartsWith("#")));

            //var scriptProps = new PhpBashBuildSnippetProperties(
            //    virtualEnvironmentName: virtualEnvName,
            //    virtualEnvironmentModule: virtualEnvModule,
            //    virtualEnvironmentParameters: virtualEnvCopyParam,
            //    packagesDirectory: packageDir,
            //    disableCollectStatic: disableCollectStatic);
            //string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.PythonSnippet, scriptProps, _logger);

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = ComposerInstallLine,
            };
        }

        public bool IsEnabled(ScriptGeneratorContext ctx)
        {
            return ctx.EnablePhp;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PhpConstants.PhpName] = targetPlatformVersion;
            }
        }

        public void SetVersion(ScriptGeneratorContext context, string version)
        {
            context.PhpVersion = version;
        }
    }
}