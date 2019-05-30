// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodePackageScriptsChecker : IChecker
    {
        private readonly IEnvironment _env;

        public NodePackageScriptsChecker([CanBeNull] IEnvironment env)
        {
            _env = env;
        }

        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            // Installing packages globally is problematic only in the App Service envelope
            if (_env?.Type == Common.EnvironmentType.AzureAppService)
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

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckScriptsForGlobalInstallationAttempts(
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

        private void CheckScript(IDictionary<string, string> scripts, string scriptKey, List<ICheckerMessage> result)
        {
            if (!scripts.ContainsKey(scriptKey))
            {
                return;
            }

            string script = scripts[scriptKey];
            if (script?.Contains("-g") == true || script?.Contains("--global") == true)
            {
                result.Add(new CheckerMessage(string.Format(Resources.Labels.NodePackageGlobalInstallMessageFormat,
                    scriptKey)));
            }
        }
    }
}
