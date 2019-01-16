// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGenerator : ILanguageScriptGenerator
    {
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodeScriptGenerator> _logger;

        public NodeScriptGenerator(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionProvider nodeVersionProvider,
            ILogger<NodeScriptGenerator> logger)
        {
            _nodeScriptGeneratorOptions = nodeScriptGeneratorOptions.Value;
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => NodeConstants.NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext context)
        {
            var requiredTools = new Dictionary<string, string>() { { "node", context.LanguageVersion } };

            var packageJson = GetPackageJsonObject(context.SourceRepo);
            string packageManagerCmd = null;
            string runBuildCommand = null;
            string runBuildAzureCommand = null;
            if (context.SourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                packageManagerCmd = NodeConstants.YarnCommand;
            }
            else
            {
                packageManagerCmd = NodeConstants.NpmCommand;
                var npmVersion = GetNpmVersion(packageJson);
                if (!string.IsNullOrEmpty(npmVersion))
                {
                    requiredTools.Add("npm", npmVersion);
                }
            }

            var packageInstallCommand = string.Format(NodeConstants.PackageInstallCommandTemplate, packageManagerCmd);

            var scriptsNode = packageJson?.scripts;
            if (scriptsNode != null)
            {
                if (scriptsNode.build != null)
                {
                    runBuildCommand = string.Format(NodeConstants.PkgMgrRunBuildCommandTemplate, packageManagerCmd);
                }

                if (scriptsNode["build:azure"] != null)
                {
                    runBuildAzureCommand = string.Format(NodeConstants.PkgMgrRunBuildAzureCommandTemplate, packageManagerCmd);
                }
            }

            if (packageJson?.dependencies != null)
            {
                Newtonsoft.Json.Linq.JObject deps = packageJson.dependencies;
                var depSpecs = deps.ToObject<IDictionary<string, string>>();
                _logger.LogDependencies(context.Language, context.LanguageVersion, depSpecs.Select(kv => kv.Key + kv.Value));
            }

            var script = new NodeBashBuildSnippet(
                packageInstallCommand: packageInstallCommand,
                runBuildCommand: runBuildCommand,
                runBuildAzureCommand: runBuildAzureCommand).TransformText();

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
                RequiredToolsVersion = requiredTools
            };
        }

        private string GetNpmVersion(dynamic packageJson)
        {
            string npmVersionRange = packageJson?.engines?.npm?.Value;
            if (npmVersionRange == null)
            {
                npmVersionRange = _nodeScriptGeneratorOptions.NpmDefaultVersion;
            }

            string npmVersion = null;
            if (!string.IsNullOrWhiteSpace(npmVersionRange))
            {
                var supportedNpmVersions = _nodeVersionProvider.SupportedNpmVersions;
                npmVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    npmVersionRange,
                    supportedNpmVersions);
                if (string.IsNullOrWhiteSpace(npmVersion))
                {
                    _logger.LogWarning("User requested npm version {npmVersion} but it wasn't resolved", npmVersionRange);
                    return null;
                }
            }

            return npmVersion;
        }

        private dynamic GetPackageJsonObject(ISourceRepo sourceRepo)
        {
            dynamic packageJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(NodeConstants.PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                // We just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
                _logger.LogInformation(ex, $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName}");
            }

            return packageJson;
        }
    }
}