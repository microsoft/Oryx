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

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreLanguageDetector : IPlatformDetector
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly DotNetCoreScriptGeneratorOptions _options;
        private readonly DefaultProjectFileProvider _projectFileProvider;
        private readonly ILogger<DotNetCoreLanguageDetector> _logger;

        public DotNetCoreLanguageDetector(
            IDotNetCoreVersionProvider versionProvider,
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            DefaultProjectFileProvider projectFileProvider,
            ILogger<DotNetCoreLanguageDetector> logger)
        {
            _versionProvider = versionProvider;
            _options = options.Value;
            _projectFileProvider = projectFileProvider;
            _logger = logger;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
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

            var version = GetVersion(context, targetFramework);
            version = GetMaxSatisfyingVersionAndVerify(version);

            return new PlatformDetectorResult
            {
                Platform = DotNetCoreConstants.LanguageName,
                PlatformVersion = version,
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

        private string GetVersion(RepositoryContext context, string targetFramework)
        {
            if (context.DotNetCoreRuntimeVersion != null)
            {
                return context.DotNetCoreRuntimeVersion;
            }

            if (_options.DotNetVersion != null)
            {
                return _options.DotNetVersion;
            }

            var version = DetermineRuntimeVersion(targetFramework);
            if (version != null)
            {
                return version;
            }

            return GetDefaultVersionFromProvider();
        }

        private string GetDefaultVersionFromProvider()
        {
            return _versionProvider.GetDefaultRuntimeVersion();
        }

        private string GetMaxSatisfyingVersionAndVerify(string runtimeVersion)
        {
            var versionMap = _versionProvider.GetSupportedVersions();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                runtimeVersion,
                versionMap.Keys);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    DotNetCoreConstants.LanguageName,
                    runtimeVersion,
                    versionMap.Keys);
                _logger.LogError(
                    exception,
                    $"Exception caught, the version '{runtimeVersion}' is not supported for the .NET Core platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }
    }
}