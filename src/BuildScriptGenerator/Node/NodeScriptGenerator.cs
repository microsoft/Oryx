// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

        public string SupportedLanguageName => Constants.NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;

            var benvArgs = $"node={context.LanguageVersion}";

            string packageInstallCommand = null;
            string npmRunBuildCommand = null;
            string npmRunBuildAzureCommand = null;
            if (context.SourceRepo.FileExists(Constants.YarnLockFileName))
            {
                packageInstallCommand = Constants.YarnInstallCommand;
            }
            else
            {
                var packageJson = GetPackageJsonObject(context.SourceRepo);
                var npmVersion = GetNpmVersion(packageJson);
                if (!string.IsNullOrEmpty(npmVersion))
                {
                    benvArgs += $" npm={npmVersion}";
                }
                packageInstallCommand = Constants.NpmInstallCommand;

                var scriptsNode = packageJson?.scripts;
                if (scriptsNode != null)
                {
                    if (scriptsNode.build != null)
                    {
                        npmRunBuildCommand = Constants.NpmRunBuildCommand;
                    }

                    if (scriptsNode["build:azure"] != null)
                    {
                        npmRunBuildAzureCommand = Constants.NpmRunBuildAzureCommand;
                    }
                }
            }

            _logger.LogDebug("Using benv args: {BenvArgs}", benvArgs);
            script = new NodeBashBuildScript(
                packageInstallCommand,
                npmRunBuildCommand,
                npmRunBuildAzureCommand,
                benvArgs).TransformText();
            return true;
        }

        private string GetNpmVersion(dynamic packageJson)
        {
            if (packageJson?.dependencies != null)
            {
                Newtonsoft.Json.Linq.JObject deps = packageJson.dependencies;
                _logger.LogEvent("ReadPackageJson", deps.ToObject<IDictionary<string, string>>());
            }

            var npmVersionRange = packageJson?.engines?.npm?.Value;
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
                var jsonContent = sourceRepo.ReadFile(Constants.PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                // We just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
                _logger.LogInformation(ex, $"Exception caught while trying to deserialize {Constants.PackageJsonFileName}");
            }

            return packageJson;
        }
    }
}