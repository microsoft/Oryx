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

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal static class ProjectFileHelpers
    {
        public static bool IsAspNetCoreWebApplicationProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            return IsAspNetCoreWebApplicationProject(projFileDoc);
        }

        public static bool IsAzureFunctionsProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            return IsAzureFunctionsProject(projFileDoc);
        }

        // To enable unit testing
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

        internal static bool IsAspNetCoreWebApplicationProject(XDocument projectFileDoc)
        {
            return IsOfSdkProjectType(
                projectFileDoc,
                DotNetCoreConstants.DotNetWebSdkName.ToLowerInvariant());
        }

        // To enable unit testing
        internal static bool IsAzureFunctionsProject(XDocument projectFileDoc)
        {
            bool isDotNetSdk = IsOfSdkProjectType(
                projectFileDoc,
                DotNetCoreConstants.AzureFunctionsPackageReferenceName.ToLowerInvariant());
            if (!isDotNetSdk)
            {
                return false;
            }

            return HasAzureFunctionsPackageReference(projectFileDoc);
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

        private static bool HasAzureFunctionsPackageReference(XDocument projectFileDoc)
        {
            var packageReferences = GetPackageReferences(projectFileDoc);
            if (packageReferences == null || !packageReferences.Any())
            {
                return false;
            }

            var packageReference = packageReferences.Where(reference =>
            {
                var referenceName = reference.Value;
                return referenceName.Equals(DotNetCoreConstants.AzureFunctionsPackageReferenceName);
            }).FirstOrDefault();

            return packageReference != null;
        }

        private static IEnumerable<XElement> GetPackageReferences(XDocument projectFileDoc)
        {
            var packageReferenceElements = projectFileDoc.XPathSelectElements(
                DotNetCoreConstants.PackageReferenceXPathExpression);
            if (packageReferenceElements == null || !packageReferenceElements.Any())
            {
                return null;
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

                    return true;
                });
            return packageReferences;
        }
    }
}
