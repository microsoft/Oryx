// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeLanguageDetector : ILanguageDetector
    {
        private static readonly string[] IisStartupFiles = new[]
        {
            "default.htm",
            "default.html",
            "default.asp",
            "index.htm",
            "index.html",
            "iisstart.htm",
            "default.aspx",
            "index.php"
        };

        private static readonly string[] TypicalNodeDetectionFiles = new[]
        {
            "server.js",
            "app.js"
        };

        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly ILogger<NodeLanguageDetector> _logger;

        public NodeLanguageDetector(
            INodeVersionProvider nodeVersionProvider,
            IOptions<NodeScriptGeneratorOptions> options,
            ILogger<NodeLanguageDetector> logger)
        {
            _nodeVersionProvider = nodeVersionProvider;
            _nodeScriptGeneratorOptions = options.Value;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            bool isNodeApp = false;

            if (sourceRepo.FileExists(NodeConstants.PackageJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.PackageLockJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                isNodeApp = true;
            }
            else
            {
                _logger.LogDebug($"Could not find {NodeConstants.PackageJsonFileName}/{NodeConstants.PackageLockJsonFileName}/{NodeConstants.YarnLockFileName} in repo");
            }

            if (!isNodeApp)
            {
                // Copying the logic currently running in Kudu:
                var mightBeNode = false;
                foreach (var typicalNodeFile in TypicalNodeDetectionFiles)
                {
                    if (sourceRepo.FileExists(typicalNodeFile))
                    {
                        mightBeNode = true;
                        break;
                    }
                }

                if (mightBeNode)
                {
                    // Check if any of the known iis start pages exist
                    // If so, then it is not a node.js web site otherwise it is
                    foreach (var iisStartupFile in IisStartupFiles)
                    {
                        if (sourceRepo.FileExists(iisStartupFile))
                        {
                            _logger.LogDebug("App in repo is not a Node.js app as it has the file {iisStartupFile}", iisStartupFile);
                            return null;
                        }
                    }

                    isNodeApp = true;
                }
                else
                {
                    _logger.LogDebug("Could not find typical Node.js files in repo"); // No point in logging the actual file list, as it's constant
                }
            }

            if (isNodeApp)
            {
                var packageJson = GetPackageJsonObject(sourceRepo);
                var nodeVersion = DetectNodeVersion(packageJson);

                return new LanguageDetectorResult
                {
                    Language = NodeConstants.NodeJsName,
                    LanguageVersion = nodeVersion,
                };
            }
            else
            {
                _logger.LogDebug("App in repo is not a Node.js app");
            }

            return null;
        }

        private string DetectNodeVersion(dynamic packageJson)
        {
            var nodeVersionRange = packageJson?.engines?.node?.Value as string;
            if (nodeVersionRange == null)
            {
                nodeVersionRange = _nodeScriptGeneratorOptions.NodeJsDefaultVersion;
            }

            string nodeVersion = null;
            if (!string.IsNullOrWhiteSpace(nodeVersionRange))
            {
                nodeVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    nodeVersionRange,
                    _nodeVersionProvider.SupportedNodeVersions);

                if (string.IsNullOrWhiteSpace(nodeVersion))
                {
                    var exc = new UnsupportedVersionException($"Target Node.js version '{nodeVersionRange}' is unsupported. " +
                        $"Supported versions are: {string.Join(", ", _nodeVersionProvider.SupportedNodeVersions)}");
                    _logger.LogError(exc, "Exception caught");
                    throw exc;
                }
            }

            return nodeVersion;
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
                _logger.LogError(ex, $"An error occurred while trying to deserialize {NodeConstants.PackageJsonFileName}");
            }

            return packageJson;
        }
    }
}