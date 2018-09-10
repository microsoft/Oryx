// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Text;
    using BuildScriptGenerator.SourceRepo;
    using Newtonsoft.Json;

    internal class NodeScriptBuilder : IBuildScriptBuilder
    {
        private INodeVersionProvider _versionProvider;
        private ISourceRepo _sourceRepo;
        private INodeSettings _nodeSettings;
        private dynamic _packageJson;

        private dynamic GetPackageJsonObject()
        {
            if (_packageJson == null)
            {
                try
                {
                    var jsonContent = _sourceRepo.ReadFile("package.json");
                    _packageJson = JsonConvert.DeserializeObject(jsonContent);
                }
                catch
                {
                    // we just ignore errors, so we leave malformed package.json
                    // files for node.js to handle, not us. This prevents us from
                    // erroring out when node itself might be able to tolerate some errors
                    // in the package.json file.
                }
            }

            return _packageJson;
        }

        public NodeScriptBuilder(ISourceRepo sourceRepo, INodeSettings nodeSettings, INodeVersionProvider nodeVersionProvider)
        {
            _sourceRepo = sourceRepo;
            _nodeSettings = nodeSettings;
            _versionProvider = nodeVersionProvider;
        }

        public string GenerateShScript()
        {
            var scriptBuilder = new StringBuilder();

            var nodeVersion = DetectNodeVersion();
            var npmVersion = DetectNpmVersion();

            scriptBuilder.AppendLine("#!/bin/bash");

            scriptBuilder.Append("source /usr/local/bin/benv ");
            if (!string.IsNullOrEmpty(nodeVersion))
            {
                scriptBuilder.Append($"node={nodeVersion} ");
            }
            if (!string.IsNullOrEmpty(npmVersion))
            {
                scriptBuilder.Append($"npm={npmVersion} ");
            }
            scriptBuilder.AppendLine();

            scriptBuilder.AppendLine("# Install npm packages");
            scriptBuilder.AppendLine($"cd \"{_sourceRepo.RootPath}\"");
            var installCommand = "eval npm install --production";
            scriptBuilder.AppendLine($"echo \"Running {installCommand}\"");
            scriptBuilder.AppendLine(installCommand);

            return scriptBuilder.ToString();
        }

        private string DetectNodeVersion()
        {
            var packageJson = GetPackageJsonObject();
            var nodeVersionRange = packageJson?.engines?.node?.Value as string;
            if (nodeVersionRange == null)
            {
                nodeVersionRange = _nodeSettings.NodeJsDefaultVersion;
            }
            string nodeVersion = null;
            if (!string.IsNullOrWhiteSpace(nodeVersionRange))
            {
                nodeVersion = _versionProvider.GetSupportedNodeVersion(nodeVersionRange);
            }
            return nodeVersion;
        }

        private string DetectNpmVersion()
        {
            var packageJson = GetPackageJsonObject();
            string npmVersionRange = packageJson?.engines?.npm?.Value;
            if (npmVersionRange == null)
            {
                npmVersionRange = _nodeSettings.NpmDefaultVersion;
            }
            string npmVersion = null;
            if (!string.IsNullOrWhiteSpace(npmVersionRange))
            {
                npmVersion = _versionProvider.GetSupportedNpmVersion(npmVersionRange);
            }
            return npmVersion;
        }
    }
}