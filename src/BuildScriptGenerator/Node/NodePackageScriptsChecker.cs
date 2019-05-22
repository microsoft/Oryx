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
        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            dynamic packageJson = NodePlatform.GetPackageJsonObject(repo, null);
            if (packageJson == null)
            {
                return null;
            }

            return CheckInstallScripts(packageJson.scripts?.ToObject<IDictionary<string, string>>());
        }

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools) =>
            Enumerable.Empty<ICheckerMessage>();

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckInstallScripts([CanBeNull] IDictionary<string, string> scripts)
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
            if (script?.Contains("npm install -g") == true || script?.Contains("npm i -g") == true)
            {
                result.Add(new CheckerMessage(
                    $"The script '{scriptKey}', defined in {NodeConstants.PackageJsonFileName}, seems to be trying " +
                    $"to install packages globally. This is unsupported by Oryx.", Extensions.Logging.LogLevel.Error));
            }
        }
    }
}
