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
        private const string NodeJsName = "nodejs";
        private const string PackageJsonFileName = "package.json";
        private const string PackageLockJsonFileName = "package-lock.json";
        private const string YarnLockFileName = "yarn.lock";

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

            if (sourceRepo.FileExists(PackageJsonFileName) ||
                sourceRepo.FileExists(PackageLockJsonFileName) ||
                sourceRepo.FileExists(YarnLockFileName))
            {
                isNodeApp = true;
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find file '{PackageJsonFileName}' or '{PackageLockJsonFileName}' or" +
                    $" '{YarnLockFileName}' in the source directory.");
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
                            _logger.LogDebug(
                                "Application in source directory is not a Node app as it has one of " +
                                $"the following files: {string.Join(", ", IisStartupFiles)}");

                            return null;
                        }
                    }
                    isNodeApp = true;
                }
                else
                {
                    _logger.LogDebug(
                        $"Could not find following typical node files in the source directory: " +
                        string.Join(", ", TypicalNodeDetectionFiles));
                }
            }

            if (isNodeApp)
            {
                var packageJson = GetPackageJsonObject(sourceRepo);
                var nodeVersion = DetectNodeVersion(packageJson);

                return new LanguageDetectorResult
                {
                    Language = NodeJsName,
                    LanguageVersion = nodeVersion,
                };
            }
            else
            {
                _logger.LogDebug("Application in source directory is not a Node app.");
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
                    var message = $"The target Node.js version '{nodeVersionRange}' is not supported. " +
                        $"Supported versions are: {string.Join(", ", _nodeVersionProvider.SupportedNodeVersions)}";

                    _logger.LogError(message);
                    throw new UnsupportedVersionException(message);
                }
            }
            return nodeVersion;
        }

        private dynamic GetPackageJsonObject(ISourceRepo sourceRepo)
        {
            dynamic packageJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                // we just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
                _logger.LogError(
                    ex,

                    $"An error occurred while trying to deserialize the '{PackageJsonFileName}' file.");
            }

            return packageJson;
        }
    }
}
