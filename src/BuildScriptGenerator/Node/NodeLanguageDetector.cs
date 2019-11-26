// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;
using SemVer;

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
            "index.php",
        };

        private static readonly string[] TypicalNodeDetectionFiles = new[]
        {
            "server.js",
            "app.js",
        };

        private readonly INodeVersionProvider _versionProvider;
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly ILogger<NodeLanguageDetector> _logger;
        private readonly IStandardOutputWriter _writer;

        public NodeLanguageDetector(
            INodeVersionProvider nodeVersionProvider,
            IOptions<NodeScriptGeneratorOptions> options,
            ILogger<NodeLanguageDetector> logger,
            IStandardOutputWriter writer)
        {
            _versionProvider = nodeVersionProvider;
            _nodeScriptGeneratorOptions = options.Value;
            _logger = logger;
            _writer = writer;
        }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            bool isNodeApp = false;

            var sourceRepo = context.SourceRepo;
            if (sourceRepo.FileExists(NodeConstants.PackageJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.PackageLockJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                isNodeApp = true;
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find {NodeConstants.PackageJsonFileName}/{NodeConstants.PackageLockJsonFileName}" +
                    $"/{NodeConstants.YarnLockFileName} in repo");
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
                                "App in repo is not a Node.js app as it has the file {iisStartupFile}",
                                iisStartupFile.Hash());
                            return null;
                        }
                    }

                    isNodeApp = true;
                }
                else
                {
                    // No point in logging the actual file list, as it's constant
                    _logger.LogDebug("Could not find typical Node.js files in repo");
                }
            }

            if (isNodeApp)
            {
                var packageJson = NodePlatform.GetPackageJsonObject(sourceRepo, _logger);
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
            var nodeVersion = packageJson?.engines?.node?.Value as string;
            if (nodeVersion == null)
            {
                return _nodeScriptGeneratorOptions.NodeJsDefaultVersion;
            }

            var matchingRange = SemanticVersionResolver.GetMatchingRange(
                nodeVersion,
                _versionProvider.SupportedNodeVersions);
            if (!matchingRange.Equals(SemanticVersionResolver.NoRangeMatch))
            {
                return matchingRange.ToString();
            }

            _logger.LogError($"The version '{nodeVersion}' is not supported for the node platform.");
            throw new UnsupportedVersionException(
                NodeConstants.NodeJsName,
                nodeVersion,
                _versionProvider.SupportedNodeVersions);
        }
    }
}