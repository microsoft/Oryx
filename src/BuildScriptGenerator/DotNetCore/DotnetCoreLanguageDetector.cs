// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotnetCoreLanguageDetector : ILanguageDetector
    {
        private readonly IDotnetCoreVersionProvider _versionProvider;
        private readonly DotnetCoreScriptGeneratorOptions _scriptGeneratorOptions;
        private readonly IAspNetCoreWebAppProjectFileProvider _aspNetCoreWebAppProjectFileProvider;
        private readonly ILogger<DotnetCoreLanguageDetector> _logger;

        public DotnetCoreLanguageDetector(
            IDotnetCoreVersionProvider versionProvider,
            IOptions<DotnetCoreScriptGeneratorOptions> options,
            IAspNetCoreWebAppProjectFileProvider aspNetCoreWebAppProjectFileProvider,
            ILogger<DotnetCoreLanguageDetector> logger)
        {
            _versionProvider = versionProvider;
            _scriptGeneratorOptions = options.Value;
            _aspNetCoreWebAppProjectFileProvider = aspNetCoreWebAppProjectFileProvider;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            var projectFile = _aspNetCoreWebAppProjectFileProvider.GetProjectFile(sourceRepo);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var projectFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotnetCoreConstants.TargetFrameworkElementXPathExpression);
            var targetFramework = targetFrameworkElement?.Value;
            if (string.IsNullOrEmpty(targetFramework))
            {
                _logger.LogDebug(
                    $"Could not find 'TargetFramework' element in the project file '{projectFile}'.");
                return null;
            }

            // If a repo explicitly specifies an sdk version, then just use it as it is.
            string languageVersion = null;
            if (sourceRepo.FileExists(DotnetCoreConstants.GlobalJsonFileName))
            {
                var globalJson = GetGlobalJsonObject(sourceRepo);
                var sdkVersion = globalJson?.sdk?.version?.Value as string;
                if (sdkVersion != null)
                {
                    languageVersion = sdkVersion;
                }
            }

            if (string.IsNullOrEmpty(languageVersion))
            {
                languageVersion = DetermineSdkVersion(targetFramework);
            }

            if (languageVersion == null)
            {
                _logger.LogDebug(
                    $"Could not find a {DotnetCoreConstants.LanguageName} version corresponding to 'TargetFramework'" +
                    $" '{targetFramework}'.");
                return null;
            }

            languageVersion = VerifyAndResolveVersion(languageVersion);

            return new LanguageDetectorResult
            {
                Language = DotnetCoreConstants.LanguageName,
                LanguageVersion = languageVersion
            };
        }

        internal string DetermineSdkVersion(string targetFramework)
        {
            switch (targetFramework)
            {
                case DotnetCoreConstants.NetCoreApp10:
                case DotnetCoreConstants.NetCoreApp11:
                    return "1.1";

                case DotnetCoreConstants.NetCoreApp20:
                case DotnetCoreConstants.NetCoreApp21:
                    return "2.1";

                case DotnetCoreConstants.NetCoreApp22:
                    return "2.2";

                case DotnetCoreConstants.NetCoreApp30:
                    return "3.0";
            }

            return null;
        }

        private string VerifyAndResolveVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return _scriptGeneratorOptions.DefaultVersion;
            }
            else
            {
                var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    version,
                    _versionProvider.SupportedDotNetCoreVersions);

                if (string.IsNullOrEmpty(maxSatisfyingVersion))
                {
                    var exc = new UnsupportedVersionException(
                        $"Target .NET Core version '{version}' is unsupported. Supported versions are:" +
                        $" {string.Join(", ", _versionProvider.SupportedDotNetCoreVersions)}");
                    _logger.LogError(exc, "Exception caught");
                    throw exc;
                }

                return maxSatisfyingVersion;
            }
        }

        private dynamic GetGlobalJsonObject(ISourceRepo sourceRepo)
        {
            dynamic globalJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(DotnetCoreConstants.GlobalJsonFileName);
                globalJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                // We just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
                _logger.LogError(
                    ex,
                    $"An error occurred while trying to deserialize {DotnetCoreConstants.GlobalJsonFileName}");
            }

            return globalJson;
        }
    }
}