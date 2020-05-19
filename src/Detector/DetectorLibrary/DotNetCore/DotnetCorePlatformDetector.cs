// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Detector.Exceptions;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    internal class DotNetCorePlatformDetector : IPlatformDetector
    {
        private readonly DotNetCoreVersionProvider _versionProvider;
        private readonly DefaultProjectFileProvider _projectFileProvider;
        private readonly ILogger<DotNetCorePlatformDetector> _logger;

        public DotNetCorePlatformDetector(
            DotNetCoreVersionProvider versionProvider,
            DefaultProjectFileProvider projectFileProvider,
            ILogger<DotNetCorePlatformDetector> logger)
        {
            _versionProvider = versionProvider;
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
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string runtimeVersion)
        {
            var versionMap = _versionProvider.GetSupportedVersions();

            // Since our semantic versioning library does not work with .NET Core preview version format, here
            // we do some trivial way of finding the latest version which matches a given runtime version
            // Runtime versions are usually like: 1.0, 2.1, 3.1, 5.0 etc.
            // (these are constructed from netcoreapp21, netcoreapp31 etc.)
            // Preview version of sdks also have preview versions of runtime versions and hence they
            // have '-' in their names.
            var nonPreviewRuntimeVersions = versionMap.Keys.Where(version => version.IndexOf("-") < 0);
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                runtimeVersion,
                nonPreviewRuntimeVersions);

            // Check if a preview version is available
            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                // NOTE:
                // Preview versions: 5.0.0-preview.3.20214.6, 5.0.0-preview.2.20160.6, 5.0.0-preview.1.20120.5
                var previewRuntimeVersions = versionMap.Keys
                    .Where(version => version.IndexOf("-") >= 0)
                    .Where(version => version.StartsWith(runtimeVersion))
                    .OrderByDescending(version => version);
                if (previewRuntimeVersions.Any())
                {
                    maxSatisfyingVersion = previewRuntimeVersions.First();
                }
            }

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    DotNetCoreConstants.PlatformName,
                    runtimeVersion,
                    versionMap.Keys);
                _logger.LogError(
                    exception,
                    $"Exception caught, the version '{runtimeVersion}' is not supported for the .NET Core platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
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
            if (context.ResolvedDotNetCoreRuntimeVersion != null)
            {
                return context.ResolvedDotNetCoreRuntimeVersion;
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

    }
}