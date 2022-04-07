// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodeDependencyChecker : IChecker
    {
        // Lists packages that should not be used, but were NOT marked as "deprecated" in npm itself.
        private static readonly IDictionary<string, string> SupersededPackages = new Dictionary<string, string>
        {
            // According to https://www.npmjs.com/package/eslint-plugin-jsx-ally:
            // "DO NOT INSTALL THIS PACKAGE. Please install eslint-plugin-jsx-a11y"
            { "eslint-plugin-jsx-ally", "eslint-plugin-jsx-a11y" },
        };

        private readonly ILogger<NodeDependencyChecker> logger;

        public NodeDependencyChecker(ILogger<NodeDependencyChecker> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            dynamic packageJson = NodePlatform.GetPackageJsonObject(repo, this.logger);
            if (packageJson == null)
            {
                this.logger.LogDebug(
                    $"{NodeConstants.PackageJsonFileName.Hash()} is null; skipping checking for superseded packages");
                return Enumerable.Empty<ICheckerMessage>();
            }

            var result = new List<ICheckerMessage>();
            CheckPackageJsonDependencyObject(packageJson.dependencies, "dependencies", result);
            CheckPackageJsonDependencyObject(packageJson.devDependencies, "devDependencies", result);
            return result;
        }

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools) =>
            Enumerable.Empty<ICheckerMessage>();

        private static void CheckPackageJsonDependencyObject(
            dynamic packageJsonChildObj,
            string childObjKey,
            List<ICheckerMessage> result)
        {
            if (packageJsonChildObj == null)
            {
                return;
            }

            Newtonsoft.Json.Linq.JObject depsObj = packageJsonChildObj;
            foreach (string packageName in depsObj.ToObject<IDictionary<string, string>>().Keys)
            {
                if (SupersededPackages.ContainsKey(packageName))
                {
                    result.Add(new CheckerMessage(string.Format(
                        Resources.Labels.NodeDependencyCheckerMessageFormat,
                        packageName,
                        childObjKey,
                        SupersededPackages[packageName])));
                }
            }
        }
    }
}
