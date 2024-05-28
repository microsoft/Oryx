// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects
    /// ASP.NET Core Web Application projects, .NET Core Azure Functions projects and
    /// ASP.NET Core Blazor Client projects.
    /// </summary>
    public class DotNetCoreDetector : IDotNetCorePlatformDetector
    {
        private readonly DefaultProjectFileProvider projectFileProvider;
        private readonly ILogger<DotNetCoreDetector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetCoreDetector"/> class.
        /// </summary>
        /// <param name="projectFileProvider">The <see cref="DefaultProjectFileProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger{DotNetCoreDetector}"/>.</param>
        public DotNetCoreDetector(DefaultProjectFileProvider projectFileProvider, ILogger<DotNetCoreDetector> logger)
        {
            this.projectFileProvider = projectFileProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var projectFile = this.projectFileProvider.GetRelativePathToProjectFile(context);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var sourceRepo = context.SourceRepo;
            var appDirectory = Path.GetDirectoryName(projectFile);
            var installAOTWorkloads = false;
            var projectFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotNetCoreConstants.TargetFrameworkElementXPathExpression);
            var targetFramework = targetFrameworkElement?.Value;
            if (string.IsNullOrEmpty(targetFramework))
            {
                this.logger.LogDebug(
                    $"Could not find 'TargetFramework' element in the project file.");
                return null;
            }

            var outputTypeElement = projectFileDoc.XPathSelectElement(
                DotNetCoreConstants.OutputTypeXPathExpression);
            var outputType = GetOutputType(outputTypeElement);

            var version = this.GetVersion(targetFramework);

            // Any Blazor WebAssembly app should have the workload installed.
            // https://github.com/microsoft/Oryx/issues/1026
            if (ProjectFileHelpers.IsBlazorWebAssemblyProject(projectFileDoc))
            {
                installAOTWorkloads = true;
            }

            return new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = version,
                ProjectFile = projectFile,
                AppDirectory = appDirectory,
                OutputType = outputType,
                InstallAOTWorkloads = installAOTWorkloads,
            };
        }

        internal static string DetermineRuntimeVersion(string targetFramework)
        {
            // Ex: "netcoreapp2.2" => "2.2"
            targetFramework = targetFramework
                .ToLower()
                .Replace("netcoreapp", string.Empty)

                // For handling .NET 5
                .Replace("net", string.Empty);

            // Support .NET moniker aliases
            // Ex: "472" => "4.72"
            //     "48" => "4.8"
            if (!targetFramework.Contains('.'))
            {
                targetFramework = targetFramework.Insert(1, ".");
            }

            // Ex: "2.2" => 2.2
            if (decimal.TryParse(targetFramework, out var val))
            {
                return val.ToString();
            }

            return null;
        }

        private static string GetOutputType(XElement outputTypeElement)
        {
            string outputType = outputTypeElement?.Value;

            // default OutputType is "Library"
            string outputTypeResult = string.IsNullOrEmpty(outputType) ? DotNetCoreConstants.DefaultOutputType : outputType;
            return outputTypeResult;
        }

        private string GetVersion(string targetFramework)
        {
            var version = DetermineRuntimeVersion(targetFramework);
            if (version != null)
            {
                return version;
            }

            this.logger.LogDebug(
                   $"Could not determine runtime version from target framework. ");
            return null;
        }
    }
}
