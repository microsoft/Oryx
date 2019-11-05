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
using Microsoft.Oryx.Common.Extensions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreLanguageDetector : ILanguageDetector
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly DotNetCoreScriptGeneratorOptions _scriptGeneratorOptions;
        DefaultProjectFileProvider _projectFileProvider;
        private readonly ILogger<DotNetCoreLanguageDetector> _logger;
        private readonly IStandardOutputWriter _writer;

        public DotNetCoreLanguageDetector(
            IDotNetCoreVersionProvider versionProvider,
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            DefaultProjectFileProvider projectFileProvider,
            ILogger<DotNetCoreLanguageDetector> logger,
            IStandardOutputWriter writer)
        {
            _versionProvider = versionProvider;
            _scriptGeneratorOptions = options.Value;
            _projectFileProvider = projectFileProvider;
            _logger = logger;
            _writer = writer;
        }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            var projectFile = _projectFileProvider.GetRelativePathToProjectFile(context);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var sourceRepo = context.SourceRepo;
            var projectFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotNetCoreConstants.TargetFrameworkElementXPathExpression);
            var targetFramework = targetFrameworkElement?.Value;
            if (string.IsNullOrEmpty(targetFramework))
            {
                _logger.LogDebug(
                    $"Could not find 'TargetFramework' element in the project file.");
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
                languageVersion = DetermineRuntimeVersion(targetFramework);
            }

            if (languageVersion == null)
            {
                _logger.LogDebug(
                    $"Could not find a {DotNetCoreConstants.LanguageName} core runtime version " +
                    $"corresponding to 'TargetFramework' '{targetFramework}'.");
                return null;
            }

            languageVersion = VerifyAndResolveVersion(languageVersion);

            return new LanguageDetectorResult
            {
                Language = DotNetCoreConstants.LanguageName,
                LanguageVersion = languageVersion,
            };
        }

        internal string DetermineRuntimeVersion(string targetFramework)
        {
            // Ex: "netcoreapp2.2" => "2.2"
            targetFramework = targetFramework.Replace(
                "netcoreapp",
                string.Empty,
                StringComparison.OrdinalIgnoreCase);

            // Ex: "2.2" => 2.2
            if (decimal.TryParse(targetFramework, out var val))
            {
                return val.ToString();
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
                        DotNetCoreConstants.LanguageName,
                        version,
                        _versionProvider.SupportedDotNetCoreVersions);
                    _logger.LogError(exc, $"Exception caught, the given version '{version}' is not supported for the .NET Core platform.");
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
                    $"An error occurred while trying to deserialize {DotNetCoreConstants.GlobalJsonFileName.Hash()}");
            }

            return globalJson;
        }
    }
}