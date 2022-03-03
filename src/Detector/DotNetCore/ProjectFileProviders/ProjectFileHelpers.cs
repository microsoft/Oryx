// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    internal static class ProjectFileHelpers
    {
        public static bool IsAspNetCoreWebApplicationProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = GetXmlDocument(sourceRepo, projectFile);
            return IsAspNetCoreWebApplicationProject(projFileDoc);
        }

        public static bool IsAzureFunctionsProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = GetXmlDocument(sourceRepo, projectFile);
            return IsAzureFunctionsProject(projFileDoc);
        }

        public static bool IsAzureBlazorWebAssemblyProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = GetXmlDocument(sourceRepo, projectFile);
            return IsBlazorWebAssemblyProject(projFileDoc);
        }

        public static string GetRelativePathToRoot(string projectFilePath, string repoRoot)
        {
            var repoRootDir = new DirectoryInfo(repoRoot);
            var projectFileInfo = new FileInfo(projectFilePath);
            var currDir = projectFileInfo.Directory;
            var parts = new List<string>();
            parts.Add(projectFileInfo.Name);

            // Since directory names are case sensitive on non-Windows OSes, try not to use ignore case
            while (!string.Equals(currDir.FullName, repoRootDir.FullName, StringComparison.Ordinal))
            {
                parts.Insert(0, currDir.Name);
                currDir = currDir.Parent;
            }

            return Path.Combine(parts.ToArray());
        }

        public static bool IsAspNetCoreWebApplicationProject(XDocument projectFileDoc)
        {
            return !IsBlazorWebAssemblyProject(projectFileDoc)
                && IsOfSdkProjectType(
                projectFileDoc,
                DotNetCoreConstants.DotNetWebSdkName.ToLowerInvariant());
        }

        public static bool IsAzureFunctionsProject(XDocument projectFileDoc)
        {
            var azureFunctionsVersionElement = projectFileDoc.XPathSelectElement(
                DotNetCoreConstants.AzureFunctionsVersionElementXPathExpression);
            var azureFunctionsVersion = azureFunctionsVersionElement?.Value;
            if (!string.IsNullOrEmpty(azureFunctionsVersion))
            {
                return true;
            }

            return HasPackageReference(projectFileDoc, DotNetCoreConstants.AzureFunctionsPackageReference);
        }

        public static bool IsBlazorWebAssemblyProject(XDocument projectFileDoc)
        {
            return HasPackageReference(projectFileDoc, DotNetCoreConstants.AzureBlazorWasmPackageReference);
        }

        private static XDocument GetXmlDocument(ISourceRepo sourceRepo, string projectFile)
        {
            return XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
        }

        private static bool IsOfSdkProjectType(XDocument projectFileDoc, string expectedSdkName)
        {
            // Look for the attribute value on Project element first as that is more common
            // Example: <Project Sdk="Microsoft.NET.Sdk/1.0.0">
            var sdkAttributeValue = projectFileDoc.XPathEvaluate(
                DotNetCoreConstants.ProjectSdkAttributeValueXPathExpression);
            var sdkName = sdkAttributeValue as string;
            if (!string.IsNullOrEmpty(sdkName) &&
                sdkName.StartsWith(expectedSdkName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Example:
            // <Project>
            //    <Sdk Name="Microsoft.NET.Sdk" Version="1.0.0" />
            var sdkNameAttributeValue = projectFileDoc.XPathEvaluate(
                DotNetCoreConstants.ProjectSdkElementNameAttributeValueXPathExpression);
            sdkName = sdkNameAttributeValue as string;
            return sdkName.EqualsIgnoreCase(expectedSdkName);
        }

        private static bool HasPackageReference(XDocument projectFileDoc, string packageName)
        {
            var packageReferenceElements = projectFileDoc.XPathSelectElements(
                DotNetCoreConstants.PackageReferenceXPathExpression);
            if (packageReferenceElements == null || !packageReferenceElements.Any())
            {
                return false;
            }

            var packageReferences = packageReferenceElements
                .Where(packageRefElement =>
                {
                    if (!packageRefElement.HasAttributes)
                    {
                        return false;
                    }

                    var includeAttribute = packageRefElement.Attributes()
                    .Where(attr => string.Equals(attr.Name.LocalName, "Include"))
                    .FirstOrDefault();
                    if (includeAttribute == null)
                    {
                        return false;
                    }

                    return includeAttribute.Value.EqualsIgnoreCase(packageName);
                });
            return packageReferences.Any();
        }
    }
}
