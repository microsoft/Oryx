// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGenerator : ILanguageScriptGenerator
    {
        private const string NodeJsName = "nodejs";
        private const string PackageJsonFileName = "package.json";

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

        public string SupportedLanguageName => NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;

            var benvArgs = $"node={context.LanguageVersion} ";

            var npmVersion = GetNpmVersion(context);
            if (!string.IsNullOrEmpty(npmVersion))
            {
                benvArgs += $"npm={npmVersion} ";
            }

            var installCommand = "eval npm install --production";

            script = new NodeBashBuildScript(installCommand, benvArgs).TransformText();

            return true;
        }

        private string GetNpmVersion(ScriptGeneratorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo);

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
                var jsonContent = sourceRepo.ReadFile(PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch
            {
                // we just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
            }

            return packageJson;
        }
    }
}