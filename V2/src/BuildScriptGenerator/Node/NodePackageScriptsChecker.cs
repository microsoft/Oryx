// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodePackageScriptsChecker : IChecker
    {
        public static readonly Regex NpmGlobalPattern = new Regex(@"\bnpm[^&|;#]+\s\-?\-g(lobal)?\b");

        private readonly IEnvironment env;

        public NodePackageScriptsChecker([CanBeNull] IEnvironment env)
        {
            this.env = env;
        }

        [NotNull]
        public static IEnumerable<ICheckerMessage> CheckScriptsForGlobalInstallationAttempts(
            [CanBeNull] IDictionary<string, string> scripts)
        {
            if (scripts == null || scripts.Count == 0)
            {
                return Enumerable.Empty<ICheckerMessage>();
            }

            var result = new List<ICheckerMessage>();
            CheckScript(scripts, "preinstall", result);
            CheckScript(scripts, "install", result);
            CheckScript(scripts, "postinstall", result);
            return result;
        }

        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            // Installing packages globally is problematic only in the App Service envelope
            if (this.env?.Type == Common.EnvironmentType.AzureAppService)
            {
                dynamic packageJson = NodePlatform.GetPackageJsonObject(repo, logger: null);
                if (packageJson != null)
                {
                    return CheckScriptsForGlobalInstallationAttempts(
                        packageJson.scripts?.ToObject<IDictionary<string, string>>());
                }
            }

            return Enumerable.Empty<ICheckerMessage>();
        }

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools) =>
            Enumerable.Empty<ICheckerMessage>();

        private static void CheckScript(IDictionary<string, string> scripts, string scriptKey, List<ICheckerMessage> result)
        {
            scripts.TryGetValue(scriptKey, out var script);
            if (script != null && NpmGlobalPattern.IsMatch(script))
            {
                result.Add(new CheckerMessage(string.Format(
                    Resources.Labels.NodePackageGlobalInstallMessageFormat,
                    scriptKey)));
            }
        }
    }
}
