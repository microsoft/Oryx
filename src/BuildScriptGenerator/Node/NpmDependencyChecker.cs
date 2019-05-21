// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NpmToolName)]
    public class NpmDependencyChecker : IChecker
    {
        // Lists packages that should not be used, but were NOT marked as "deprecated" in npm itself.
        private static readonly IDictionary<string, string> SupersededPackages = new Dictionary<string, string>
        {
            // According to https://www.npmjs.com/package/eslint-plugin-jsx-ally:
            // "DO NOT INSTALL THIS PACKAGE. Please install eslint-plugin-jsx-a11y"
            { "eslint-plugin-jsx-ally", "eslint-plugin-jsx-a11y" }
        };

        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            dynamic packageJson = NodePlatform.GetPackageJsonObject(repo, null);
            if (packageJson == null)
            {
                return null;
            }

            var result = new List<ICheckerMessage>();
            CheckPackageJsonDependencyObject(packageJson.dependencies, "dependencies", result);
            CheckPackageJsonDependencyObject(packageJson.devDependencies, "devDependencies", result);
            return result;
        }

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools) =>
            Enumerable.Empty<ICheckerMessage>();

        private static void CheckPackageJsonDependencyObject(
            dynamic packageJsonObj,
            string packageJsonKey,
            List<ICheckerMessage> result)
        {
            if (packageJsonObj == null)
            {
                return;
            }

            Newtonsoft.Json.Linq.JObject depsObj = packageJsonObj;
            foreach (string packageName in depsObj.ToObject<IDictionary<string, string>>().Keys)
            {
                if (SupersededPackages.ContainsKey(packageName))
                {
                    result.Add(new CheckerMessage(
                        $"The package '{packageName}', specified in {NodeConstants.PackageJsonFileName}'s " +
                        $"{packageJsonKey}, is known to have been superseded by {SupersededPackages[packageName]}. " +
                        "Consider switching over."));
                }
            }
        }
    }
}
