// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodePlatform : IProgrammingPlatform
    {
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodePlatform> _logger;
        private readonly NodeLanguageDetector _detector;

        public NodePlatform(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionProvider nodeVersionProvider,
            ILogger<NodePlatform> logger,
            NodeLanguageDetector detector)
        {
            _nodeScriptGeneratorOptions = nodeScriptGeneratorOptions.Value;
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
            _detector = detector;
        }

        public string Name => NodeConstants.NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            string runBuildCommand = null;
            string runBuildAzureCommand = null;

            string packageManagerCmd = context.SourceRepo.FileExists(NodeConstants.YarnLockFileName) ? NodeConstants.YarnCommand : NodeConstants.NpmCommand;
            _logger.LogInformation("Using {packageManager}", packageManagerCmd);

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
                _logger.LogDependencies(context.Language, context.NodeVersion, depSpecs.Select(kv => kv.Key + kv.Value));
            }

            var scriptProps = new NodeBashBuildSnippetProperties(
                packageInstallCommand: packageInstallCommand,
                runBuildCommand: runBuildCommand,
                runBuildAzureCommand: runBuildAzureCommand);
            string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeSnippet, scriptProps, _logger);

            return new BuildScriptSnippet { BashBuildScriptSnippet = script };
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return !repo.DirExists(NodeConstants.NodeModulesDirName);
        }

        public bool IsEnabled(ScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableNodeJs;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            Debug.Assert(sourceRepo != null, $"{nameof(sourceRepo)} must not be null since Node needs access to the repository");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion["node"] = targetPlatformVersion;
            }

            var packageJson = GetPackageJsonObject(sourceRepo, _logger);
            if (packageJson != null)
            {
                var npmVersion = GetNpmVersion(packageJson);
                if (!string.IsNullOrEmpty(npmVersion))
                {
                    toolsToVersion["npm"] = npmVersion;
                }
            }
        }

        public void SetVersion(ScriptGeneratorContext context, string version)
        {
            context.NodeVersion = version;
        }

        internal static dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
        {
            dynamic packageJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(NodeConstants.PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception exc)
            {
                // Leave malformed package.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                logger.LogWarning(exc, $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName}");
            }

            return packageJson;
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
    }
}