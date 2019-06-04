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
    internal class DotNetCoreLanguageDetector : ILanguageDetector
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly DotNetCoreScriptGeneratorOptions _scriptGeneratorOptions;
        private readonly IAspNetCoreWebAppProjectFileProvider _aspNetCoreWebAppProjectFileProvider;
        private readonly ILogger<DotNetCoreLanguageDetector> _logger;

        public DotNetCoreLanguageDetector(
            IDotNetCoreVersionProvider versionProvider,
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            IAspNetCoreWebAppProjectFileProvider aspNetCoreWebAppProjectFileProvider,
            ILogger<DotNetCoreLanguageDetector> logger)
        {
            _versionProvider = versionProvider;
            _scriptGeneratorOptions = options.Value;
            _aspNetCoreWebAppProjectFileProvider = aspNetCoreWebAppProjectFileProvider;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            var projectFile = _aspNetCoreWebAppProjectFileProvider.GetRelativePathToProjectFile(sourceRepo);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var projectFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotNetCoreConstants.TargetFrameworkElementXPathExpression);
            var targetFramework = targetFrameworkElement?.Value;
            if (string.IsNullOrEmpty(targetFramework))
            {
                _logger.LogDebug(
                    $"Could not find 'TargetFramework' element in the project file '{projectFile}'.");
                return null;
            }

            // If a repo explicitly specifies an sdk version, then just use it as it is.
            string languageVersion = null;
            if (sourceRepo.FileExists(DotNetCoreConstants.GlobalJsonFileName))
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
                    $"Could not find a {DotNetCoreConstants.LanguageName} version corresponding to 'TargetFramework'" +
                    $" '{targetFramework}'.");
                return null;
            }

            languageVersion = VerifyAndResolveVersion(languageVersion);

            return new LanguageDetectorResult
            {
                Language = DotNetCoreConstants.LanguageName,
                LanguageVersion = languageVersion
            };
        }

        internal string DetermineSdkVersion(string targetFramework)
        {
            switch (targetFramework)
            {
                case DotNetCoreConstants.NetCoreApp10:
                    return DotNetCoreRuntimeVersions.NetCoreApp10;

                case DotNetCoreConstants.NetCoreApp11:
                    return DotNetCoreRuntimeVersions.NetCoreApp11;

                case DotNetCoreConstants.NetCoreApp20:
                    return DotNetCoreRuntimeVersions.NetCoreApp20;

                case DotNetCoreConstants.NetCoreApp21:
                    return DotNetCoreRuntimeVersions.NetCoreApp21;

                case DotNetCoreConstants.NetCoreApp22:
                    return DotNetCoreRuntimeVersions.NetCoreApp22;

                case DotNetCoreConstants.NetCoreApp30:
                    return DotNetCoreRuntimeVersions.NetCoreApp30;
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
                        $"Target .NET Core runtime version '{version}' is unsupported. Supported versions are:" +
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
                var jsonContent = sourceRepo.ReadFile(DotNetCoreConstants.GlobalJsonFileName);
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
                    $"An error occurred while trying to deserialize {DotNetCoreConstants.GlobalJsonFileName}");
            }

            return globalJson;
        }
    }
}